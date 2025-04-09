using System.Web;
using CodeReviewServices;
using GitLabWebhook.CodeReviewServices;
using GitLabWebhook.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GitLabWebhook.CodeReviewServices.Strategies;
using System.Net.Http;
using GitLabWebhook.CodeReviewServices.Decorators;
using System.Text;

namespace GitLabWebhook.Controllers
{
    /// <summary>
    /// Class to hold the response from GitLab webhook
    /// </summary>
    public class GitLabWebhookResponse
    {
        /// <summary>
        /// Status of the response
        /// </summary>
        public string Status { get; set; } = String.Empty;

        /// <summary>
        /// Message returned in the response
        /// </summary>
        public string Message { get; set; } = String.Empty;
    }

    /// <summary>
    /// Controller for processing GitLab webhook requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GitLabWebhookController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIService _openAiService;
        private readonly GitLabService _gitLabService;
        private readonly string _gitlabBaseURL;

        private readonly JiraService _jiraService;
        private readonly ConfluenceService _confluenceService;
        private readonly string _hostURL;
        private readonly string? _repoAllowListContainsText;
        private readonly string? _repoDisallowListContainsText;
        private readonly IPromptGenerationStrategyFactory _strategyFactory;



        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabWebhookController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public GitLabWebhookController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IPromptGenerationStrategyFactory strategyFactory)
        {
            _httpClient = new HttpClient(); // Create a single instance of HttpClient
            _openAiService = new OpenAIService();
            _gitLabService = new GitLabService(configuration, httpClientFactory);
            _jiraService = new JiraService(configuration);
            _confluenceService = new ConfluenceService(configuration,httpClientFactory);
            _hostURL = configuration["Host:HostURL"] ?? throw new ArgumentNullException("Host:HostURL");
            _gitlabBaseURL = configuration["GitLab:ApiBaseUrl"] ?? throw new ArgumentNullException("GitLab:ApiBaseUrl"); 
            _repoAllowListContainsText = configuration["Allowlist:Contains"];
            _repoDisallowListContainsText = configuration["Disallowlist:Contains"];
            _strategyFactory = strategyFactory;
        }


        /// <summary>
        /// Retrieves the sanity check for the target branch in GitLab.
        /// </summary>
        /// <param name="url">The URL of the merge request.</param>
        /// <returns>An asynchronous operation that, upon completion, returns an IActionResult.</returns>
        [HttpGet("GetTargetBranchSanityCheckGitLabHack")]
        public async Task<IActionResult> GetTargetBranchSanityCheckGitLabHack(string url)
        {
            await PostTargetBranchSanityCheck(url);
            return Content("<html><body><script>window.close();</script></body></html>", "text/html");

        }

        /// <summary>
        /// This is a delegate off the main Webhook controller (another service, not this one) posts a sanity check for the target branch in GitLab.
        /// </summary>
        /// <param name="url">The URL of the merge request.</param>
        [HttpPost("postTargetBranchSanityCheck")]
        public async Task<IActionResult> PostTargetBranchSanityCheck(string url)
        {

            //TODO: Validate URL

            // 1. Get Target Branch from MR (could be primary, or could be release branch, based on strategy)
            MRDetails mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);

            if(mrDetails == null || mrDetails.JIRA == null)
            {
                return Ok("No JIRA ticket or MR details. Expecting JIRA#PROJ-123; in title");
            }

            // Limit this to only a few repositories to start with
            if (_repoAllowListContainsText !=null && !mrDetails.TargetRepoPath.Contains(_repoAllowListContainsText))
            {
                return Ok($"Sorry, project {mrDetails.TargetRepoPath} is not on the allowed list.");
            }

            if (_repoDisallowListContainsText !=null && mrDetails.TargetRepoPath.Contains(_repoDisallowListContainsText))
            {
                return Ok($"Sorry, project {mrDetails.TargetRepoPath} is explicitly disallowed as it doesn't follow the standard release strategy.");
            }

            // 2. Get Target Release from JIRA
            var jiraTargetRelease = await _jiraService.GetReleaseTarget(mrDetails.JIRA);

            // fy26-0403
            var standardizedJiraReleaseTarget = StringParserService.ConvertJIRAToConfluence(jiraTargetRelease);

            string targetBranchFromConfluence = string.Empty;

            try
            {
                targetBranchFromConfluence = await _confluenceService.GetTargetBranch(standardizedJiraReleaseTarget);
            }
            catch
            {
                  // Target branch not found, that's fine, there probably is not one available
            }

            targetBranchFromConfluence = string.IsNullOrEmpty(targetBranchFromConfluence) ? "NONE!!!" : targetBranchFromConfluence;

            var mrTargetBranch = mrDetails.TargetBranch;

           // Posting image to gitlab
           // curl--request POST --form "file=@/path_to_your_image.png" "https://gitlab.example.com/api/v4/projects/:id/uploads" - H "PRIVATE-TOKEN: your_access_token"
           // curl--request POST --data "body=Here is an inline image: ![Image](https://gitlab.example.com/uploads/your-image-id/image.png)" "https://gitlab.example.com/api/v4/projects/:id/merge_requests/:merge_request_iid/notes" - H "PRIVATE-TOKEN: your_access_token"


            if (mrTargetBranch != targetBranchFromConfluence)
            {
              
                string tableBadResponse =
                      $@" ### Branch Sanity Check - FAIL 
[[Retrigger Check]({_hostURL}/api/GitLabWebhook/GetTargetBranchSanityCheckGitLabHack?url={HttpUtility.UrlEncode(url)})]
                        
|         **System**              |       **Target**         |
|---------------------------------|--------------------------|
|  **JIRA Release Target**        | {jiraTargetRelease}      |  
|  **JIRA->Confluence Branch** | {targetBranchFromConfluence}|
|  **MR Target Branch**           | {mrDetails.TargetBranch} |

:no_entry: Please confirm the correct target branch in your MR or Confluence [FT4 - Application Lifecycle Management (ALM) Strategy](https://confluence.dell.com/display/DSA/FT4+-+Application+Lifecycle+Management+%28ALM%29+Strategy) ";


                DiscussionDetail? discussion = await _gitLabService.FindExistingDiscussion(mrDetails.MRId, mrDetails.TargetRepoPath, "### Branch Sanity Check - FAIL");
                if (discussion != null)
                {
                    // Don't duplicate the comment.
                    return Ok("Bad review comment already exists, not adding duplicate.");
                }

                await _gitLabService.PostCommentToMR(tableBadResponse, mrDetails.MRId, mrDetails.TargetRepoPath, true, GitLabService.COMMENT_TYPE_DISCUSSION_NOTE);

                return Ok(tableBadResponse);
            }

            string goodMRResponse = $":white_check_mark: Target Branch from MR: {mrDetails.TargetBranch} vs Target Branch from Confluence: {targetBranchFromConfluence} " +
               $"vs Release Target from JIRA {jiraTargetRelease}\n";
            

            string tableGoodResponse =
                        $@" ### Branch Sanity Check - PASS
                        
|         **System**              |       **Target**         |
|---------------------------------|--------------------------|
|  **JIRA Release Target**        | {jiraTargetRelease}      |  
|  **JIRA->Confluence Branch** | {targetBranchFromConfluence}|
|  **MR Target Branch**           | {mrDetails.TargetBranch} |

:white_check_mark: JIRA Target -> Confluence Table -> Merge Request Target ";



            // If good note exists, don't repost it
            var matchingNote = await _gitLabService.FindExistingNote(mrDetails.MRId, mrDetails.TargetRepoPath, "### Branch Sanity Check - PASS");

            if (matchingNote != null)
            {
                // Don't duplicate the comment.
                return Ok("Good review comment already exists, not adding duplicate.");
            }

            // If bad review exists.. Dismiss it
            await _gitLabService.DismissReview("Branches look good, this comment has been dismissed.", mrDetails.MRId, mrDetails.TargetRepoPath);

            await _gitLabService.PostCommentToMR(tableGoodResponse, mrDetails.MRId, mrDetails.TargetRepoPath);

            return Ok(tableGoodResponse);            
        }

        /// <summary>
        /// What Gitlab calls when MR Event happens
        /// </summary>
        /// <param name="webhookPayload"></param>
        /// <returns></returns>
        [HttpPost("sink")]
        public async Task<IActionResult> PostGitLabWebhook([FromBody] JObject webhookPayload)
        {
            Console.WriteLine(webhookPayload);

            // Extracting the MR and project details from the webhook payload using JObject
            var mrObject = webhookPayload["object_attributes"];
            
            if (mrObject == null){
                throw new Exception("Not able to extract MR object from webhook payload");
            }

            // Get MR details from the payload
            string mrTitle = mrObject["title"]?.ToString() ?? string.Empty;
            string mrSourceBranch = mrObject["source_branch"]!.ToString();
            string mrTargetBranch = mrObject["target_branch"]!.ToString();
            string mrAuthor = mrObject["author_id"]!.ToString();
            string mrAssignee = mrObject["assignee_id"]?.ToString() ?? "None";
            string mrID = mrObject["iid"]!.ToString(); // Merge Request ID
            string mrUrl = mrObject["url"]!.ToString(); // MR URL
            string gitLabApiToken = Environment.GetEnvironmentVariable("GITLABTOKEN") ?? throw new ArgumentNullException("GITLABTOKEN needs to be set in the environment");
            string targetRepoPath = mrObject["target"]!["path_with_namespace"]!.ToString();

            // Call to GetChangedFilesAsyncAndLeaveFeedback
            // Construct the URL for fetching the changes from the GitLab API
            string url =
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/changes";

            Console.WriteLine(_gitlabBaseURL);
            Console.WriteLine(url);
            Console.WriteLine(gitLabApiToken);

            // Set the Private-Token header for the first API call
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("private-token", gitLabApiToken);

            // Send GET request to fetch the diffs using the shared HttpClient
            var response = await _httpClient.GetStringAsync(url);

            Console.WriteLine(response.ToString());

            // Parse the response JSON using JObject
            JObject jsonResponse = JObject.Parse(response);

            // Extract the diff_refs object (which contains base_sha, start_sha, head_sha)
            var diffRefs = jsonResponse["diff_refs"];

            if (diffRefs == null){
                throw new Exception($"Not able to extract diff_refs from MR {mrUrl}");
            }

            string baseSha = diffRefs["base_sha"]!.ToString();
            string headSha = diffRefs["head_sha"]!.ToString();
            string startSha = diffRefs["start_sha"]!.ToString();
            var changes = jsonResponse["changes"] ?? throw new Exception($"Not able to extract changes from MR {mrUrl}");

            var change = changes[0]; // just take first change for now

            string fileName = change!["new_path"]!.ToString();
            string oldFileName = change["old_path"]!.ToString();
            string diff = change["diff"]!.ToString();

            var diffLines = diff.Split('\n');
       

            var fileDiff = new FileDiff
            {
                FileName = fileName,
                BaseSha = baseSha,
                HeadSha = headSha,
                StartSha = startSha,
                Diff = diff,
                HasSuggestion = false

            };


            // Post the feedback as inline comments to GitLab
            await _gitLabService.PostReviewFeedbackOnSpecificLine(
                fileName,
                baseSha,
                startSha,
                headSha,
                0,
                "Line one looks good",
                mrID,
                targetRepoPath,
                gitLabApiToken
            );

            return Ok(
                new GitLabWebhookResponse
                {
                    Status = "received",
                    Message = "Merge Request Event Processed",
                }
            );
        }


        string GetDirectoryFromFileName(string fileName)
        {
            int lastSlash = fileName.LastIndexOf('/');
            return lastSlash >= 0 ? fileName.Substring(0, lastSlash) : "";
        }


        public static int EstimateTokenCount(string text, double charsPerToken = 3.5)
        {
            return (int)Math.Ceiling((double)text.Length / charsPerToken);
        }

        [HttpGet("CodeReviewfeedback")]
        public async Task<IActionResult> CodeReviewfeedback(string mrURL, StrategyType strategyType)
        {
            var mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(mrURL);
            var baseStrategy = _strategyFactory.GetStrategy(strategyType);
            if (baseStrategy == null)
            {
                return BadRequest("Invalid strategy type.");
            }

            var codeContent = mrDetails.GetAllFileDiffsWithFullContent();

            List<string> changedFiles = new List<string>();

            mrDetails.fileDiffs.ForEach(fileDiff =>
            {
                changedFiles.Add(fileDiff.FileName);
            });  

          
            // TODO refactor this, just testing out tokenization

            var framework = FrameworkDetector.DetectFrameworkFromFiles(changedFiles);

            var finalStrategy = baseStrategy; // For most types, it's going to be the same as baseStrategy

            if (strategyType == StrategyType.FewShot){
                finalStrategy  = framework switch
                {
                    CodeFramework.Angular => new AngularPromptDecorator(baseStrategy),
                    CodeFramework.DotNet => new DotNetPromptDecorator(baseStrategy),
                    _ => baseStrategy
                };
            }

            var grouped = mrDetails.fileDiffs
            .GroupBy(fd => GetDirectoryFromFileName(fd.FileName))
            .ToList();
            
            var sbretval = new StringBuilder();
            var sbBuffer = new StringBuilder();
            int tokenBuffer = 0;
            int maxTokens = 3000; // This is an assumption, I need to figure out how many I actually have

            foreach (var group in grouped)
            {
                foreach (var fileDiff in group)
                {
                    string fileText = fileDiff.GetFileNameAndDiff();
                    int tokens = EstimateTokenCount(fileText);

                    if (tokenBuffer + tokens > maxTokens)
                    {
                        // Flush current buffer

                        if (finalStrategy is ICodeSmellAwarePromptGenerationStrategy smellAware)
                        {
                            foreach ( var codeSmellType in smellAware.CodeSmellTypes){
                                var promptMessages = smellAware.GetMessagesForPrompt(sbBuffer.ToString(), codeSmellType);
                                sbretval.AppendLine(await _openAiService.GetFeedback(promptMessages));                                
                            }
                        }else{
                            var promptMessages = finalStrategy.GetMessagesForPrompt(sbBuffer.ToString());
                            sbretval.AppendLine(await _openAiService.GetFeedback(promptMessages));
                        }                    

                        Console.WriteLine(sbretval.ToString());

                        // Reset buffer
                        sbBuffer.Clear();
                        tokenBuffer = 0;
                    }

                    sbBuffer.AppendLine(fileText);
                    tokenBuffer += tokens;
                }
            }

            // Flush remaining buffer (if anything is left)
            if (tokenBuffer > 0)
            {


                if (finalStrategy is ICodeSmellAwarePromptGenerationStrategy smellAware)
                {
                    foreach ( var codeSmellType in smellAware.CodeSmellTypes)
                    {
                        var promptMessages = smellAware.GetMessagesForPrompt(sbBuffer.ToString(), codeSmellType);
                        sbretval.AppendLine(await _openAiService.GetFeedback(promptMessages));                                
                    }
                }
                else
                {
                    var promptMessages = finalStrategy.GetMessagesForPrompt(sbBuffer.ToString());
                    sbretval.AppendLine(await _openAiService.GetFeedback(promptMessages));
                }  

                Console.WriteLine(sbretval.ToString());
                             

            }

            return Ok(sbretval.ToString());
        }



  
    }
}
