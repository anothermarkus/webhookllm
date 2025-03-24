using System.Text.RegularExpressions;

namespace GitLabWebhook.CodeReviewServices
{
    public class StringParserService
    {
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

        public static string GetJIRATicket(string title)
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
