using System.Text.RegularExpressions;

namespace GitLabWebhook.CodeReviewServices
{
    /// <summary>
    /// Class responsible for parsing strings.
    /// </summary>
    public class StringParserService
    {
        /// <summary>
        /// Retrieves the merge request ID from the provided URL.
        /// </summary>
        /// <param name="url">The URL of the merge request.</param>
        /// <returns>The merge request ID as a string.</returns>
        public static string? GetMergeRequestIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Handle both UI and API format
            int mrIndex = Array.IndexOf(segments, "merge_requests");
            if (mrIndex >= 0 && mrIndex < segments.Length - 1)
            {
                return segments[mrIndex + 1];
            }

            return null;
        }

         // Skip files with ".spec.ts" or "Test" in the name
        public static bool IsTestFile(string filename){             
            return (filename.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase) ||
                filename.Contains("Test", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Converts a JIRA release target to the Confluence format.
        /// </summary>
        /// <param name="jiraReleaseTarget">The JIRA release target.</param>
        /// <returns>The release target in the Confluence format.</returns>
        public static string ConvertJIRAToConfluence(string jiraReleaseTarget)
        {
            string firstPart = jiraReleaseTarget.Substring(0, 4).ToLower();  // "FY26" -> "fy26"
            string secondPart = jiraReleaseTarget.Substring(jiraReleaseTarget.Length - 5);  // "-0303"
            return $"{firstPart}{secondPart}";
        }

        public static string? GetProjectPathFromUrl(string url)
        {
            Uri uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Contains("api")) // API-style URL
            {
                int projectIndex = Array.IndexOf(segments, "projects");
                if (projectIndex >= 0 && projectIndex < segments.Length - 1)
                {
                    string encodedPath = segments[projectIndex + 1];
                    return Uri.UnescapeDataString(encodedPath);
                }
            }
            else // UI-style URL
            {
                int mergeRequestIndex = Array.IndexOf(segments, "merge_requests");
                if (mergeRequestIndex > 0)
                {
                    int end = segments[mergeRequestIndex - 1] == "-" ? mergeRequestIndex - 1 : mergeRequestIndex;
                    var pathSegments = segments.Take(end);
                    return string.Join("/", pathSegments);
                }
            }

            return null;
        }

        public static string? GetCommitIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["commit_id"];
        }




        /// <summary>
        /// Retrieves the JIRA ticket from the provided title.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>The JIRA ticket as a string, or null if no JIRA ticket is found.</returns>
        public static string? GetJIRATicket(string title)
        {
            string pattern = @"JIRA#(\w+-\d+)";

            // Find the match using Regex
            Match match = Regex.Match(title, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null; // NO JIRA ticket
        }
    }
}
