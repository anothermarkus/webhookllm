using Models;

namespace GitLabWebhook.models
{
    public class MRDetails
    {
        public string MRId { get; set; }
        public string TargetRepoPath { get; set; }

        public List<FileDiff> fileDiffs { get; set; }

    }
}
