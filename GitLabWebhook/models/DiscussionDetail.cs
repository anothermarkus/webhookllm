using Newtonsoft.Json.Linq;

namespace GitLabWebhook.models
{
    public class DiscussionDetail
    {
        public string DiscussionId { get; set; }
        public JObject Note { get; set; }
    }
}
