
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{

    public class AngularZeroShotPromptGenerationStrategy : ZeroShotPromptGenerationStrategy
    {

        public override List<ChatMessage> GetMessagesForPrompt(string code)
        {
            
            var systemMessage = OpenAPIPrompts.CodeReviewPromptAngular;
            var userMessage = OpenAPIPrompts.ZeroShotCodeSmellPromptUserInput.Replace("{code}", code);

            return new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userMessage)
            };
        }
    }

}