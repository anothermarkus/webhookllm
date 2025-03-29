using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;
using CodeReviewServices;
using GitLabWebhook.CodeReviewServices;
using GitLabWebhook.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GitLabWebhook.Controllers
{
    public class GitLabWebhookResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class GitLabWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly OpenAIService _openAiService;
        private readonly GitLabService _gitLabService;
        private readonly JiraService _jiraService;
        private readonly ConfluenceService _confluenceService;
        private readonly string _hostURL;
        private readonly string _repoAllowListContainsText;
        private readonly string _repoDisallowListContainsText;

        // Inject IConfiguration to access the app settings and initialize the HttpClient
        public GitLabWebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient(); // Create a single instance of HttpClient
            _openAiService = new OpenAIService();
            _gitLabService = new GitLabService(_configuration);
            _jiraService = new JiraService(_configuration);
            _confluenceService = new ConfluenceService(_configuration);
            _hostURL = configuration["Host:HostURL"];
            _repoAllowListContainsText = configuration["Allowlist:Contains"];
            _repoDisallowListContainsText = configuration["Disallowlist:Contains"];



        }

        /// <summary>
        /// Get a sample feedback from the OpenAI service for code review
        /// </summary>
        /// <returns>Sample feedback from OpenAI for code review</returns>
        [HttpGet("sampleopenaifeedback")]
        public async Task<IActionResult> GetSampleFeedback()
        {
            var reviewCriteria = new List<string>
            {
                "Check for unused variables",
                "Ensure all functions have docstrings",
                "Check for code duplication",
                "Ensure error handling is implemented",
            };

            var code =
                @"// Sample code
                 function foo() {
                     let x = 1;
                     console.log(x);
                 }";

            var openAIService = new OpenAIService();
            var feedback = await openAIService.ReviewCodeAsync(code);

            return Ok(feedback);
        }

        /// <summary>
        /// Retrieves details of a specific merge request based on the MR ID (or string).
        /// e.g. https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/merge_requests/1418
        ///         
        /// </summary>
        /// <param name="url">The merge request URL (string) passed as part of the route.</param>
        /// <returns>Sanity Check if target branch is valid for JIRA Ticket</returns>
        [HttpGet("getTargetBranchSanityCheck")]
        public async Task<IActionResult> GetTargetBranchSanityCheck(string url)
        {

            
            // 1. Get Target Branch from MR (could be primary, or could be release branch, based on strategy)
            MRDetails mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);
            
            // 2. Get Target Release from JIRA
            var jiraTargetRelease = await _jiraService.GetReleaseTarget(mrDetails.JIRA);

            // fy26-0403
            var standardizedJiraReleaseTarget = StringParserService.ConvertJIRAToConfluence(jiraTargetRelease);

            // primary or primary-fy26-0403
            var targetBranchFromConfluence = await _confluenceService.GetTargetBranch(standardizedJiraReleaseTarget);

            targetBranchFromConfluence = string.IsNullOrEmpty(targetBranchFromConfluence) ? "NONE!!!" : targetBranchFromConfluence;

            var mrTargetBranch = mrDetails.TargetBranch;

            

            if (mrTargetBranch != targetBranchFromConfluence)
            {
                return Ok($"You've been a naughty little MR {mrTargetBranch} does not match release: {jiraTargetRelease} target branch derived from confluence {targetBranchFromConfluence} " +
                    $"https://confluence.dell.com/display/DSA/FT4+-+Application+Lifecycle+Management+%28ALM%29+Strategy");
            }
               

            return Ok($"Target Branch from MR: {mrDetails.TargetBranch} vs Target Branch from Confluence: {targetBranchFromConfluence} " +
                $"vs Release Target from JIRA {jiraTargetRelease}\n");

        }

        [HttpGet("GetTargetBranchSanityCheckGitLabHack")]
        public async Task<IActionResult> GetTargetBranchSanityCheckGitLabHack(string url)
        {
            await PostTargetBranchSanityCheck(url);
            return Content("<html><body><script>window.close();</script></body></html>", "text/html");

        }


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
            if (!mrDetails.TargetRepoPath.Contains(_repoAllowListContainsText))
            {
                return Ok($"Sorry, project {mrDetails.TargetRepoPath} is not on the allowed list.");
            }

            if (mrDetails.TargetRepoPath.Contains(_repoDisallowListContainsText))
            {
                return Ok($"Sorry, project {mrDetails.TargetRepoPath} is explicitly disallowed as it doesn't follow the standard release strategy.");
            }

            // 2. Get Target Release from JIRA
            var jiraTargetRelease = await _jiraService.GetReleaseTarget(mrDetails.JIRA);

            // fy26-0403
            var standardizedJiraReleaseTarget = StringParserService.ConvertJIRAToConfluence(jiraTargetRelease);

            // primary or primary-fy26-0403
            string targetBranchFromConfluence = null;

            try
            {
                targetBranchFromConfluence = await _confluenceService.GetTargetBranch(standardizedJiraReleaseTarget);
            }
            catch
            {
                  
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


                DiscussionDetail discussion = await _gitLabService.FindExistingDiscussion(mrDetails.MRId, mrDetails.TargetRepoPath, "### Branch Sanity Check - FAIL");
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
        /// Retrieves details of a specific merge request based on the MR ID (or string).
        /// e.g. https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/merge_requests/1418
        ///         
        /// </summary>
        /// <param name="mrString">The merge request URL (string) passed as part of the route.</param>
        /// <returns>Details of the specified merge request NO update to the MR.</returns>
        [HttpGet("getmrDetails")]
        public async Task<IActionResult> GetMergeRequestDetails(string url)
        {
            MRDetails mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);
            return Ok(mrDetails); // Return MR details as a response
        }

        /// <summary>
        /// Generates MR comments
        /// e.g. https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/-/merge_requests/1375
        ///         
        /// </summary>
        /// <param name="mrString">The merge request URL (string) passed as part of the route.</param>
        /// <returns>LLM Generated Feedback on the MR itself</returns>
        [HttpGet("openAILLMReview")]
        public async Task<IActionResult> GetOpenAILLMReview(string url)
        {
            MRDetails mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);

            var jiraTargetBranch = await _jiraService.GetReleaseTarget(mrDetails.JIRA);
            
            
            var jsonData = JsonConvert.SerializeObject(mrDetails.fileDiffs);

            var feedback = await _openAiService.ReviewCodeAsync(jsonData);

            //TODO Start aggregating feedback from multiple sources like other feedback reviews..etc
            return Ok("JIRA Target Branch: {jiraTargetBranch}\n MR Target Branch: {mrDetails.TargetBranch}\n {feedback}"); // Return MR details as a response
        }

        [HttpPost("addopenAILLMReviewComment")]
        public async Task<IActionResult> PostOpenAILLMReviewComment(string url)
        {
            MRDetails mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);

            // Serialize the List<FileDiff> to JSON
            var jsonData = JsonConvert.SerializeObject(mrDetails.fileDiffs);

            var feedback = await _openAiService.ReviewCodeAsync(jsonData);

           

            await _gitLabService.PostCommentToMR(feedback, mrDetails.MRId, mrDetails.TargetRepoPath);

            return Ok(feedback); // Return MR details as a response
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

            // Get MR details from the payload
            string mrTitle = mrObject["title"].ToString();
            string mrSourceBranch = mrObject["source_branch"].ToString();
            string mrTargetBranch = mrObject["target_branch"].ToString();
            string mrAuthor = mrObject["author_id"].ToString();
            string mrAssignee = mrObject["assignee_id"]?.ToString() ?? "None";
            string mrID = mrObject["iid"].ToString(); // Merge Request ID
            string mrUrl = mrObject["url"].ToString(); // MR URL
            string gitLabApiBaseUrl = _configuration["GitLab:ApiBaseUrl"];
            string gitLabApiToken = Environment.GetEnvironmentVariable("GITLABTOKEN");
            string targetRepoPath = mrObject["target"]["path_with_namespace"].ToString();

            // Call to GetChangedFilesAsyncAndLeaveFeedback
            // Construct the URL for fetching the changes from the GitLab API
            string url =
                $"{gitLabApiBaseUrl}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/changes";

            Console.WriteLine(gitLabApiBaseUrl);
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
            string baseSha = diffRefs["base_sha"]?.ToString();
            string headSha = diffRefs["head_sha"]?.ToString();
            string startSha = diffRefs["start_sha"]?.ToString();
            var changes = jsonResponse["changes"]; 

            var change = changes[0]; // just take first change for now

            string fileName = change["new_path"].ToString();
            string oldFileName = change["old_path"]?.ToString();
            string diff = change["diff"].ToString();

            // We will simulate posting feedback for lines 1 and 2. We can extract line numbers from the diff.
            var diffLines = diff.Split('\n');
            //int line1 = 1; // Typically, line 1 in the diff.       



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

       
    }
}
