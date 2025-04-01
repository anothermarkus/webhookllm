using System.Text;
using GitLabWebhook.models;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitLabWebhook.CodeReviewServices
{

    /// <summary>
    /// Class responsible for providing GitLab API functionality.
    /// </summary>
    public class GitLabService
    {
        private readonly string _gitlabToken;
        private readonly string _gitlabBaseURL;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The type of comment used for discussion notes in Gitlab.
        /// </summary>
        public const string COMMENT_TYPE_DISCUSSION_NOTE = "DiscussionNote";

        /// <summary>
        /// Constructor for the GitLabService class.
        /// </summary>
        /// <param name="configuration">An instance of IConfiguration used to retrieve GitLab API token and base URL.</param>
        /// <param name="httpClientFactory">An instance of IHttpClientFactory used to create an HttpClient.</param>
        public GitLabService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _gitlabToken = Environment.GetEnvironmentVariable("GITLABTOKEN") ?? throw new ArgumentNullException("GITLABTOKEN needs to be set in the environment");
            _gitlabBaseURL = configuration["GitLab:ApiBaseUrl"] ?? throw new ArgumentNullException("GitLab:ApiToken");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("private-token", _gitlabToken);
        }


        /// <summary>
        /// Retrieves the details of a specific merge request based on the provided URL.
        /// </summary>
        /// <param name="url">The URL of the merge request.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the MRDetails object 
        /// representing the details of the merge request.</returns>
        /// <exception cref="System.Exception">Thrown if the merge request ID or the project path is null.</exception>
        public async Task<MRDetails> GetMergeRequestDetailsFromUrl(string url)
        {
            string mrId = StringParserService.GetMergeRequestIdFromUrl(url);
            string projectPath = StringParserService.GetProjectPathFromUrl(url);

            if (mrId != null && projectPath != null)
            {
                return await FetchMRDetails(url, projectPath, mrId);
            }

            throw new Exception("Not able to fetch MR Details mrID or projectPath is null");
        }

        private async Task<MRDetails> FetchMRDetails(string url, string projectPath, string mrId)
        {
        
            string apiUrl = $"{_gitlabBaseURL}/{Uri.EscapeDataString(projectPath)}/merge_requests/{mrId}/changes";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject mrDetails = JObject.Parse(responseBody);

                var source_branch = mrDetails["source_branch"];
                var target_branch = mrDetails["target_branch"]?.ToString() ?? throw new Exception($"Not able to find target branch from MR {apiUrl}");
                var diffRefs = mrDetails["diff_refs"];
                var title = mrDetails["title"]?.ToString();
                
                if (diffRefs == null){
                    throw new Exception($"Not able to find diff refs from MR {apiUrl}");
                }

                string commit_sha = mrDetails["sha"]!.ToString();
                string baseSha = diffRefs["base_sha"]!.ToString();
                string headSha = diffRefs["head_sha"]!.ToString();
                string startSha = diffRefs["start_sha"]!.ToString();

                var changes = mrDetails["changes"];

                if (changes == null){
                    throw new Exception($"Not able to changes from MR {apiUrl}");
                }

                List<FileDiff> diffs = [];

                foreach (var change in changes)
                {
                    string fileName = change["new_path"]!.ToString();
                    //string oldFileName = change["old_path"]!.ToString();
                    string diff = change["diff"]!.ToString();

                    var fileURL = $"{_gitlabBaseURL}/{Uri.EscapeDataString(projectPath)}/repository/files/{Uri.EscapeDataString(fileName)}/raw?ref={commit_sha}";

                    try
                    {
                        var fileContents = await _httpClient.GetStringAsync(fileURL);

                        var fileDiff = new FileDiff
                        {
                            FileName = fileName,
                            FileContents = fileContents,
                            BaseSha = baseSha,
                            HeadSha = headSha,
                            StartSha = startSha,
                            Diff = diff,
                            HasSuggestion = false,
                        };
                        diffs.Add(fileDiff);
                    } catch (Exception) { /* Could be 404 MRs can have a deleted file, and that's normal! */ }
                }


                return new MRDetails
                {
                    MRId = mrId,
                    TargetRepoPath = projectPath,
                    fileDiffs = diffs,
                    Title = title,
                    JIRA = StringParserService.GetJIRATicket(title?.ToString() ?? "No Title Found"),
                    TargetBranch = target_branch
                };

               
            }

            throw new Exception($"Not able to fetch changes from mr {apiUrl}");
        }

        /// <summary>
        /// Retrieves the files associated with a merge request.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="mergeRequestURL">The URL of the merge request.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the response from the server as a string.</returns>

        public async Task<string> GetMergeRequestFiles(string projectId, string mergeRequestURL)
        {
            var response = await _httpClient.GetStringAsync(mergeRequestURL);
            return response;
        }


        /// <summary>
        /// Post as comment to MR, rather than a specific line
        /// </summary>
        /// <param name="comment">The comment to be posted.</param>
        /// <param name="mrID">The ID of the merge request.</param>
        /// <param name="targetRepoPath">The path of the target repository.</param>
        /// <param name="isblocking">Indicates whether the comment is blocking.</param>
        /// <param name="commentType">The type of the comment.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        //
        // Returns: Http Response
        public async Task PostCommentToMR(string comment, string mrID, string targetRepoPath, bool isblocking = false, string? commentType = null)
        {
            var commentBody = new { body = comment,
                                    type = commentType };

            var jsonContent = JsonConvert.SerializeObject(commentBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url =
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes?per_page=100&page=1";

            if (isblocking)
            {
                url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions?per_page=100&page=1";
            }

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// FindExistingNote is an asynchronous method that searches for an existing note in a GitLab merge request.
        /// </summary>
        /// <param name="mrID">The ID of the merge request.</param>
        /// <param name="targetRepoPath">The path of the target repository.</param>
        /// <param name="matchingCommentSnippet">The snippet of the comment to match.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the matching note as a JObject, or null if no matching note is found.</returns>
        public async Task<JObject?> FindExistingNote(string mrID, string targetRepoPath, string matchingCommentSnippet)
        {
            string url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes?per_page=100&page=1";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var notes = JsonConvert.DeserializeObject<List<dynamic>>(content);

            if (notes == null)  {
                return null; // No Notes.
            }
            JObject? matchingNote = null;

            foreach (JObject note in notes)
            {
                string body = note["body"]?.ToString() ?? String.Empty;

                if (body.Contains(matchingCommentSnippet))
                {
                    matchingNote = note;
                    return matchingNote;
                }
            }

            return matchingNote;
        }



        /// <summary>
        /// Finds an existing discussion in a GitLab merge request based on a matching comment snippet.
        /// </summary>
        /// <param name="mrID">The ID of the merge request.</param>
        /// <param name="targetRepoPath">The path of the target repository.</param>
        /// <param name="matchingCommentSnippet">The snippet of the comment to match.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the matching discussion as a DiscussionDetail object, or null if no matching discussion is found.</returns>/
        public async Task<DiscussionDetail?> FindExistingDiscussion(string mrID, string targetRepoPath, string matchingCommentSnippet)
        {
            string url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions?per_page=100&page=1"; // TODO traverse
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var discussions = JsonConvert.DeserializeObject<List<dynamic>>(content);

            if (discussions == null)
            {
                // No Discussions
                return null;
            }

            string? discussionId = null;
            JObject? matchingNote = null;

            foreach (JObject discussion in discussions)
            {
                JArray? notes = discussion["notes"] as JArray;

                if (notes == null){
                    continue;
                }

                foreach (JObject note in notes)
                {
                    string? body = note["body"]?.ToString();

                    if (body !=null && body.Contains(matchingCommentSnippet))
                    {
                        discussionId = discussion["id"]?.ToString();
                        matchingNote = note;
                        break; // Exit the loop once a match is found
                    }
                }
            }

            if (matchingNote == null)
            {
                // No note with the specified type found.
                return null;
            }

            var noteId = matchingNote["id"];

            return new DiscussionDetail
            {
                DiscussionId = discussionId,
                Note = matchingNote
            };

        }
        
         /// <summary>Dismisss a review by updating the discussion note with the given dismissal message, mrID, and targetRepoPath.</summary> 
         /// <param name="dismissalMessage">The message to be added to the discussion note.</param> <param name="mrID">The merge request ID.</param> 
         /// <param name="targetRepoPath">The path of the target repository.</param>
         /// <returns>A Task representing the asynchronous operation.</returns> 
        public async Task DismissReview(string dismissalMessage, string mrID, string targetRepoPath)
        {
            DiscussionDetail? discussion = await FindExistingDiscussion(mrID, targetRepoPath, "### Branch Sanity Check - FAIL");

            // No discussion note to dismiss exiting...
            if (discussion == null) { return; }

            var matchingNote = discussion.Note ?? throw new Exception("No matching note found.");
            var discussionId = discussion.DiscussionId;
            var noteId = matchingNote["id"] ?? throw new Exception("No matching noteId found.");


            var updateNoteBody = new
            {
                body = $"{matchingNote["body"]?.ToString()}\n\n[{dismissalMessage}]"
            };

            // 1. Add a Resolve Note instead of having a two step process
            // TODO: Consolidate -- &body={System.Web.HttpUtility.UrlEncode(dismissalMessage)
            string updateNoteUrl = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes/{noteId}";
            var jsonContent = JsonConvert.SerializeObject(updateNoteBody);
            var contentToUpdate = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var updateResponse = await _httpClient.PutAsync(updateNoteUrl, contentToUpdate);
            updateResponse.EnsureSuccessStatusCode();

            // 2. Dismiss The Note
            string resolveUrl = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions/{discussionId}?resolved=true";
            var resolveResponse = await _httpClient.PutAsync(resolveUrl, null);
            resolveResponse.EnsureSuccessStatusCode();
        }




        /// <summary>Posts review feedback on a specific line in a merge request.</summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="baseSha">The base SHA.</param>
        /// <param name="startSha">The start SHA.</param>
        /// <param name="headSha">The head SHA.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="mrID">The merge request ID.</param>
        /// <param name="targetRepoPath">The target repository path.</param>
        /// <param name="gitLabApiToken">The GitLab API token.</param>
        /// <returns>The HTTP response message.</returns>
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
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions?per_page=100&page=1";

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
