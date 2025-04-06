namespace GitLabWebhook.models
{

    public class CodeSmellExampleEmbeddings
    {
        public MRDetails Details { get; set; }
        public CodeSmellType CodeSmell { get; set; }
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}