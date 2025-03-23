using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GitLabWebhook.models;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeReviewServices
{
  
    public class GitLabService
    {
        private readonly string _gitlabToken;
        private readonly string _gitlabBaseURL;
        private readonly HttpClient _httpClient;
        private readonly string _repositoryBaseURL;

        public GitLabService(IConfiguration configuration)
        {
            _gitlabToken = Environment.GetEnvironmentVariable("GITLABTOKEN");
            _gitlabBaseURL = configuration["GitLab:ApiBaseUrl"];
            _repositoryBaseURL = configuration["GitLab:RepositoryBaseURL"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("private-token", _gitlabToken);
        }


        public async Task<MRDetails> GetMergeRequestDetailsFromUrl(string url)
        {
            string mrId = GetMergeRequestIdFromUrl(url);
            string projectPath = GetProjectPathFromUrl(url);

            if (mrId != null && projectPath != null)
            {
                var fileDiffs =  await FetchMRDetails(projectPath, mrId);

                return new MRDetails
                {
                    MRId = mrId,
                    TargetRepoPath = projectPath,
                    fileDiffs = fileDiffs
                };
            }

            throw new Exception("Not able to fetch MR Details");
        }

        public static string GetMergeRequestIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            return segments[segments.Length - 1]; // The last segment should be the MR ID
        }


        public static string GetProjectPathFromUrl(string url)
        {
            Uri uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');

            // Find the "merge_requests" part and exclude anything after that (including `/-/` and the merge request ID)
            int mergeRequestIndex = Array.IndexOf(segments, "merge_requests");

            // We need to handle the special case where `/-/` exists, and it's part of the URL.
            // If we find `/-/`, we need to skip that as well.
            if (mergeRequestIndex > 0 && segments[mergeRequestIndex - 1] == "-")
            {
                mergeRequestIndex--; // Exclude `/-/` by adjusting the index
            }

            // Combine everything from the beginning to before "merge_requests" (and exclude `/-/` if present)
            return string.Join("/", segments, 1, mergeRequestIndex - 1);
        }

        private async Task<List<FileDiff>> FetchMRDetails(string projectPath, string mrId)
        {
            string apiUrl = $"{_gitlabBaseURL}/{Uri.EscapeDataString(projectPath)}/merge_requests/{mrId}/changes";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject mrDetails = JObject.Parse(responseBody);

                var source_branch = mrDetails["source_branch"];
                var diffRefs = mrDetails["diff_refs"];

                string commit_sha = mrDetails["sha"]?.ToString();
                string baseSha = diffRefs["base_sha"]?.ToString();
                string headSha = diffRefs["head_sha"]?.ToString();
                string startSha = diffRefs["start_sha"]?.ToString();

                var changes = mrDetails["changes"];

                List<FileDiff> diffs = new List<FileDiff>();

                foreach (var change in changes)
                {
                    string fileName = change["new_path"].ToString();
                    string oldFileName = change["old_path"]?.ToString();
                    string diff = change["diff"].ToString();

                    var fileURL = $"{_gitlabBaseURL}/{Uri.EscapeDataString(projectPath)}/repository/files/{Uri.EscapeDataString(fileName)}/raw?ref={commit_sha}";

                    var fileContents = await _httpClient.GetStringAsync(fileURL);

                    var fileDiff = new FileDiff
                    {
                        FileName = fileName,
                        FileContents = fileContents,
                        BaseSha = baseSha,
                        HeadSha = headSha,
                        StartSha = startSha,
                        Diff = diff,
                        HasSuggestion = false

                    };
                    diffs.Add(fileDiff);
                }


                return diffs;
            }

            throw new Exception("Not able to fetch changes from mr {apiUrl}");


        }

    

        public async Task<string> GetMergeRequestFiles(string projectId, string mergeRequestURL)
        {
            var response = await _httpClient.GetStringAsync(mergeRequestURL);
            return response;
        }

        // Post as comment to MR, rather than a specific line
        public async Task PostCommentToMR(string comment, string mrID, string targetRepoPath)
        {
            var commentBody = new { body = comment };

            var jsonContent = JsonConvert.SerializeObject(commentBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url =
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        // Post on a specific line
        public async Task<HttpResponseMessage> PostReviewFeedbackOnSpecificLine(
            string fileName,
            string baseSha,
            string startSha,
            string headSha,
            int lineNumber,
            string comment,
            string mrID,
            string targetRepoPath,
            string gitLabApiToken
        )
        {
            // Construct the URL for posting the inline comment (discussion)
            string postCommentUrl =
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions";

            // Construct the comment data (inline comment on a specific line)
            // https://docs.gitlab.com/api/discussions/
            //
            var commentData = new
            {
                body = comment,
                position = new
                {
                    position_type = "text", // The position type is "text" for line comments
                    base_sha = baseSha, // The SHA for the base commit
                    start_sha = startSha, // The SHA for the start commit
                    head_sha = headSha, // The SHA for the head commit
                    new_line = lineNumber, // The new line number (after the change). This CANNOT be anything the user has not changed. Use Notes API for general purpose things.
                    new_path = fileName, // The file path
                    old_path = fileName, // The old file path (same as new path in this case)
                },
            };

            // Convert the data to JSON
            string jsonCommentData = Newtonsoft.Json.JsonConvert.SerializeObject(commentData);

            // Create the request message
            HttpRequestMessage requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                postCommentUrl
            )
            {
                Content = new StringContent(jsonCommentData, Encoding.UTF8, "application/json"),
            };

            // Add the GitLab API token as a header
            requestMessage.Headers.Add("private-token", gitLabApiToken);

            // Send the request
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            return response;
        }
    }
}
