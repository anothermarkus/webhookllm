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
        public static string GetMergeRequestIdFromUrl(string url)
        {

            Uri uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            return segments[segments.Length - 1]; // The last segment should be the MR ID
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

        /// <summary>
        /// Retrieves the project path from the provided URL.
        /// </summary>
        /// <param name="url">The URL of the project.</param>
        /// <returns>The project path as a string.</returns>
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
