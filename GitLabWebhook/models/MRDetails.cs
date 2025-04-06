namespace GitLabWebhook.Models
{
    /// <summary>
    /// Class to hold the details of a specific Merge Request
    /// </summary>
    public class MRDetails
    {
        /// <summary>
        /// The ID of the Merge Request
        /// </summary>
        public string MRId { get; set; }  = string.Empty;

        /// <summary>
        /// The path of the repository where the Merge Request is targeted
        /// </summary>
        public string TargetRepoPath { get; set; } = string.Empty;

        /// <summary>
        /// The title of the Merge Request
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The list of the files and their diffs in the Merge Request
        /// </summary>
        public List<FileDiff>? fileDiffs { get; set; }

        /// <summary>
        /// The JIRA ticket associated with the Merge Request
        /// </summary>
        public string? JIRA { get; set; }

        /// <summary>
        /// The branch that the Merge Request is targeting
        /// </summary>
        public string TargetBranch { get; set; }  = string.Empty;

        /// <summary>
        /// The string representation of the object
        /// </summary>
        /// <returns></returns>

        public string GetAllFileDiffsWithFullContent(){  
            var retval = "";
            foreach (var fileDiff in fileDiffs){
                retval += $"FileChanges [ {fileDiff.GetFileNameAndDiff()}]\n"; 
            }
            return retval;
        }
    }
}
