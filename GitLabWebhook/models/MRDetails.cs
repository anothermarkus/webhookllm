using Models;

namespace GitLabWebhook.models
{
    public class MRDetails
    {
        public string MRId { get; set; }
        public string TargetRepoPath { get; set; }
        public string Title { get; set; }
        public List<FileDiff> fileDiffs { get; set; }
        public string JIRA { get; set; }
        public string TargetBranch { get; set; }

    }
}
