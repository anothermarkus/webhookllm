namespace GitLabWebhook.models
{

    public class CodeSmellExampleEmbeddings
    {
        public MRDetails Details { get; set; }
        public CodeSmellCategory CodeSmell { get; set; }
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}