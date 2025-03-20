using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeReviewServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;
using Newtonsoft.Json.Linq;

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

        // Inject IConfiguration to access the app settings and initialize the HttpClient
        public GitLabWebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient(); // Create a single instance of HttpClient
            _openAiService = new OpenAIService();
            _gitLabService = new GitLabService(_configuration);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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

            var service = new OpenAIService();
            var feedback = await service.ReviewCodeAsync(code, reviewCriteria);

            Console.WriteLine(feedback);

            return Ok(feedback);
        }

        // POST api/gitlabwebhook
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JObject webhookPayload)
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
            await GetChangedFilesAsyncAndLeaveFeedback(
                gitLabApiBaseUrl,
                targetRepoPath,
                mrID,
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

        public async Task GetChangedFilesAsyncAndLeaveFeedback(
            string baseURL,
            string targetRepoPath,
            string mrID,
            string gitLabApiToken
        )
        {
            // Construct the URL for fetching the changes from the GitLab API
            string url =
                $"{baseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/changes";

            Console.WriteLine(baseURL);
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

            // Iterate through each change in the changes array
            // foreach (var change in changes)

            // TODO: Construction FileDiff object 

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

            // TODO: Pass object to OpenAIService to recommend changes and to which line

            // if fileDiff.HasSuggestion

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
        }
    }
}
