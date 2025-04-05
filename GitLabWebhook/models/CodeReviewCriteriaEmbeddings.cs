namespace GitLabWebhook.models
{

    public class CodeReviewCriteriaEmbeddings
    {
        public ReadOnlyMemory<float> Embedding { get; set; }

        public String Text { get; set; }
    }


}