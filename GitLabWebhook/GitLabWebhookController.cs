using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeReviewServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;
using Newtonsoft.Json;
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
            var feedback = await openAIService.ReviewCodeAsync(code, reviewCriteria);

            return Ok(feedback);
        }


        /// <summary>
        /// Retrieves details of a specific merge request based on the MR ID (or string).
        /// e.g. https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/merge_requests/1418
        ///         
        /// </summary>
        /// <param name="mrString">The merge request identifier (string) passed as part of the route.</param>
        /// <returns>Details of the specified merge request.</returns>
        [HttpGet("getmrDetails")]
        public async Task<IActionResult> GetMergeRequestDetails(string url)
        {
            List<FileDiff> mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);
            return Ok(mrDetails); // Return MR details as a response
        }

        [HttpGet("openAILLMReview")]
        public async Task<IActionResult> GetOpenAILLMReview(string url)
        {
            List<FileDiff> mrDetails = await _gitLabService.GetMergeRequestDetailsFromUrl(url);

            var reviewCriteria = new List<string>
            {
                "You are an experienced code reviewer, and I am providing you with a JSON array of file changes for a merge request (MR). Each item in the array represents a file change in the MR.",
                "Here is the JSON array of file changes (in a list). Review each file change thoroughly for potential issues, such as bugs, bad practices, inefficient code, security vulnerabilities, or any other improvements you might suggest. " +
                "For each file change, please provide the following updates in your response JSON:",
                "- `FileName`: The name of the file being reviewed. This should be provided as is.",
                "- `LLMComment`: A comment or feedback about the file change. If no comment is necessary, leave it as an empty string. If you suggest a change or improvement, provide it here.",
                "- `LineForComment`: The line number where the change is occurring, or where you suggest adding a comment. If no specific line needs a comment, set this to `0`.",
                "- `HasSuggestion`: A boolean value. Set `true` if you have a suggestion for this change or feedback, otherwise set it to `false` if there are no comments or suggestions.",
                "Your response should follow the exact structure I provide below. Please return the updated JSON array with the `FileName`, `LLMComment`, `LineForComment`, and `HasSuggestion` fields updated.",
                "Ensure that the `LineForComment` points to a specific line in the `Diff` (if applicable) or set it to `0` if no specific line needs a comment. If you have any suggestions, set `HasSuggestion` to `true`; otherwise, set it to `false`.",
                "Please respond with the updated JSON array and do not modify the structure of the original input. Only update the fields: `FileName`, `LLMComment`, `LineForComment`, and `HasSuggestion`."
            };

            // Serialize the List<FileDiff> to JSON
            var jsonData = JsonConvert.SerializeObject(mrDetails);

            var feedback = await _openAiService.ReviewCodeAsync(jsonData, reviewCriteria);


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
