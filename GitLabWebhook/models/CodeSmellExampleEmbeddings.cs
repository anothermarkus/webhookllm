namespace GitLabWebhook.Models
{

    public class CodeSmellExampleEmbeddings
    {
        public MRDetails Details { get; set; }
        //TODO: public CodeSmellType CodeSmell { get; set; }
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}