using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GitLabWebhook.CodeReviewServices;
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
        public static string COMMENT_TYPE_DISCUSSION_NOTE = "DiscussionNote";
        

        public GitLabService(IConfiguration configuration)
        {
            _gitlabToken = Environment.GetEnvironmentVariable("GITLABTOKEN");
            _gitlabBaseURL = configuration["GitLab:ApiBaseUrl"];           
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("private-token", _gitlabToken);
        }


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
                var target_branch = mrDetails["target_branch"]?.ToString();
                var diffRefs = mrDetails["diff_refs"];
                var title = mrDetails["title"]?.ToString();

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
                    } catch (Exception) { /* Could be 404 MRs can have a deleted file */ }
                }


                return new MRDetails
                {
                    MRId = mrId,
                    TargetRepoPath = projectPath,
                    fileDiffs = diffs,
                    Title = title,
                    JIRA = StringParserService.GetJIRATicket(title),
                    TargetBranch = target_branch
                };

               
            }

            throw new Exception("Not able to fetch changes from mr {apiUrl}");
        }

 
        public async Task<string> GetMergeRequestFiles(string projectId, string mergeRequestURL)
        {
            var response = await _httpClient.GetStringAsync(mergeRequestURL);
            return response;
        }

        // Post as comment to MR, rather than a specific line
     
        public async Task PostCommentToMR(string comment, string mrID, string targetRepoPath, bool isblocking = false, string commentType = null)
        {
            var commentBody = new { body = comment,
                                    type = commentType };

            var jsonContent = JsonConvert.SerializeObject(commentBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url =
                $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes";

            if (isblocking)
            {
                url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions";
            }

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<JObject> FindExistingNote(string mrID, string targetRepoPath, string matchingCommentSnippet)
        {
            string url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/notes";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var notes = JsonConvert.DeserializeObject<List<dynamic>>(content);
            JObject matchingNote = null;

            foreach (JObject note in notes)
            {
                string body = note["body"].ToString();

                if (body.Contains(matchingCommentSnippet))
                {
                    matchingNote = note;
                    return matchingNote;
                }
            }

            return null;
        }


        public async Task<DiscussionDetail> FindExistingDiscussion(string mrID, string targetRepoPath, string matchingCommentSnippet)
        {
            string url = $"{_gitlabBaseURL}/{Uri.EscapeDataString(targetRepoPath)}/merge_requests/{mrID}/discussions";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var discussions = JsonConvert.DeserializeObject<List<dynamic>>(content);

            if (discussions == null)
            {
                // No Notes
                return null;
            }

            string discussionId = null;
            JObject matchingNote = null;

            foreach (JObject discussion in discussions)
            {
                JArray notes = (JArray)discussion["notes"];

                foreach (JObject note in notes)
                {
                    string body = note["body"].ToString();

                    if (body.Contains(matchingCommentSnippet))
                    {
                        discussionId = discussion["id"].ToString();
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

        //TODO: If it's been resolved already, skip it
        public async Task DismissReview(string dismissalMessage, string mrID, string targetRepoPath)
        {
            DiscussionDetail discussion = await FindExistingDiscussion(mrID, targetRepoPath, "### Branch Sanity Check - FAIL");

            // No discussion note to dismiss exiting...
            if (discussion == null) { return; }

            var matchingNote = discussion.Note;
            var discussionId = discussion.DiscussionId;
            var noteId = matchingNote["id"];


            var updateNoteBody = new
            {
                body = $"{matchingNote["body"].ToString()}\n\n[{dismissalMessage}]"
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
