using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using CodeReviewServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
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


            //var reviewCriteria = new List<string>
            //{
            //    "You are an Enterprise Code Assistant ensuring the code follows best practices and I am providing you with a JSON array of file changes for a merge request (MR). " +
            //    "Each item in the array represents a file change in the MR.",

            //    "Review each file change thoroughly for these code standards:",

            //     // Code Style               
            //    "Ensure adherence to DRY (Don't Repeat Yourself) principle by avoiding code duplication and reusing code effectively.",
            //    "Maintain cyclomatic complexity under 10; break down complex methods into smaller, manageable ones.",
            //    "Avoid deep nesting; use early returns for cleaner logic flow.",
            //    "Use null-conditional operators (?.) and null-coalescing operators (??) for safe null value access.",
            //    "Implement guard clauses to handle null or invalid inputs early in methods.",

            //    // Memory Management
            //    "Always use 'using' statements for disposable objects to ensure automatic resource disposal.",
            //    "Minimize memory leaks by unsubscribing from events when no longer needed.",
            //    "Dispose of unmanaged resources properly and be mindful of large object retention in memory.",
            //    "Avoid unnecessary object creation; use weak references or caching where applicable.",

            //    // Error Handling
            //    "Use try-catch blocks to handle exceptions; catch specific exceptions, not generic ones.",
            //    "Always use 'finally' for cleanup operations to release resources.",
            //    "Avoid silent failures; log exceptions for troubleshooting and debugging.",
            //    "Throw custom exceptions only for business logic errors, not for regular control flow.",
            //    "Don't use exceptions for control flow; use conditional checks instead.",

            //    // Thread Handling & Async/Await
            //    "Use async/await for asynchronous programming; avoid manually managing threads.",
            //    "Use ConfigureAwait(false) to avoid deadlocks in non-UI thread operations.",
            //    "Avoid blocking async calls (e.g., don't use Result or Wait()).",
            //    "Ensure thread safety by using locks or thread-safe collections when accessing shared resources.",
            //    "Use CancellationToken for graceful cancellation of long-running async operations.",
            //    "Avoid using Thread.Sleep() in async code; prefer Task.Delay() for non-blocking waits.",

            //    "For each file change, please provide feedback in the following JSON format:",

            //    "- `FileName`: The name of the file being reviewed. This should be provided as is.",
            //    "- `LLMComment`: A comment or feedback about the file change. If no comment is necessary, leave it as an empty string. If you suggest a change or improvement, provide it here.",
            //    "- `LineForComment`: The line number where you are suggesting a change **within the context of the diff**. If there is no specific line to comment on, use 0.",

            //    "Ensure that the `LineForComment` refers to a specific line in the diff where you have feedback. If there is no suggestion, set `LineForComment` to 0.",

            //    "Please note: `string.IsNullOrWhiteSpace` is an extension method, so you do not need to manually check for null. It already handles null checks internally. Make sure not to suggest adding any additional null checks where `string.IsNullOrWhiteSpace` is used.",

            //    "The code should be self-documenting and should not require additional comments or clarification about its behavior.",

            //    "Please respond with a JSON array only. The structure should be [ `FileName`, `LLMComment`, `LineForComment` ]."
            //};

            // LLM is having trouble with line numbers...


            // gleaned from: https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions
            //List<string> prompt = new List<string>
            //{
            //    "You are an Enterprise Code Assistant. Review each code snippet below for its adherence to the following categories",
            //    "1) Code Style & Formatting",
            //    "2) Security & Compliance",
            //    "3) Error Handling & Logging",
            //    "4) Readability & Maintainability",
            //    "5) Performance & Scalability",
            //    "6) Testing & Quality Assurance",
            //    "7) Documentation & Version Control",
            //    "8) Accessibility & Internationalization",
            //    "Create a table and assign a rating of 'extraordinary', 'acceptable', or 'poor' for each category. Return a markdown table titled 'Enterprise Standards' with rows for each category and columns for 'Category' and 'Rating'",
            //    "Here are the changed file contents to analyze:"
            //};

            var reviewCriteria = new List<string>
            {
                "You are an Enterprise Code Assistant ensuring the code follows best practices and I am providing you with a JSON array of file changes for a merge request (MR). " +
                "Each item in the array represents a file change in the MR.",

                "Review each file change thoroughly for these code standards:",

                 // Code Style               
                "Ensure adherence to DRY (Don't Repeat Yourself) principle by avoiding code duplication and reusing code effectively.",
                "Maintain cyclomatic complexity under 10; break down complex methods into smaller, manageable ones.",
                "Avoid deep nesting; use early returns for cleaner logic flow.",
                "Use null-conditional operators (?.) and null-coalescing operators (??) for safe null value access.",
                "Implement guard clauses to handle null or invalid inputs early in methods.",

                // Memory Management
                "Always use 'using' statements for disposable objects to ensure automatic resource disposal.",
                "Minimize memory leaks by unsubscribing from events when no longer needed.",
                "Dispose of unmanaged resources properly and be mindful of large object retention in memory.",
                "Avoid unnecessary object creation; use weak references or caching where applicable.",

                // Error Handling
                "Use try-catch blocks to handle exceptions; catch specific exceptions, not generic ones.",
                "Always use 'finally' for cleanup operations to release resources.",
                "Avoid silent failures; log exceptions for troubleshooting and debugging.",
                "Throw custom exceptions only for business logic errors, not for regular control flow.",
                "Don't use exceptions for control flow; use conditional checks instead.",

                // Thread Handling & Async/Await
                "Use async/await for asynchronous programming; avoid manually managing threads.",
                "Use ConfigureAwait(false) to avoid deadlocks in non-UI thread operations.",
                "Avoid blocking async calls (e.g., don't use Result or Wait()).",
                "Ensure thread safety by using locks or thread-safe collections when accessing shared resources.",
                "Use CancellationToken for graceful cancellation of long-running async operations.",
                "Avoid using Thread.Sleep() in async code; prefer Task.Delay() for non-blocking waits.",

                "For each file change, please provide feedback in the following format:",

                "FileName: The name of the file being reviewed. ",
                "Comment: A comment or feedback about the file chang.  If no comment is necessary, leave it as an empty string. If you suggest a change or improvement, provide it here along with the line number.",
               
                "Please note: `string.IsNullOrWhiteSpace` is an extension method, so you do not need to manually check for null. It already handles null checks internally. Make sure not to suggest adding any additional null checks where `string.IsNullOrWhiteSpace` is used.",

                "The code should be self-documenting and should not require additional comments or clarification about its behavior.",
                "The code should be self-documenting and should not require additional comments or clarification about its behavior.",
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
