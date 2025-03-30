using Newtonsoft.Json.Linq;

namespace GitLabWebhook.models
{
    /// <summary>
    /// Represents a discussion in GitLab, which is a group of notes.
    /// </summary>
    public class DiscussionDetail
    {
        /// <summary>
        /// The ID of the discussion.
        /// </summary>
        public string? DiscussionId { get; set; }
        /// <summary>
        /// The note contained in the discussion.
        /// </summary>
        public JObject? Note { get; set; }
    }
}
