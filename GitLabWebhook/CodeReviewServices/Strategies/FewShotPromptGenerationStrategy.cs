 
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
 {
 
    public class FewShotPromptGenerationStrategy : ICodeSmellAwarePromptGenerationStrategy, IPromptGenerationStrategy
    {

        public virtual List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<Enum> CodeSmellTypes => throw new NotImplementedException();
        
        public virtual List<ChatMessage> GetMessagesForPrompt(string code)
        {
            throw new NotImplementedException();
        }
    
    }

}
    