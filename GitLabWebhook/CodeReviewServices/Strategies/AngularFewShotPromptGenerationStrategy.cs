
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{

    public class AngularFewShotPromptGenerationStrategy : FewShotPromptGenerationStrategy
    {
        public override IEnumerable<Enum> CodeSmellTypes => Enum.GetValues(typeof(AngularCodeSmellType)).Cast<Enum>();

        public override List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
        {
            var definition = AngularCodeSmells.GetDefinition((AngularCodeSmellType)codeSmellType);
            var systemMessage = OpenAPIPrompts.GetPositiveFewShotCodeSmellSystemMessageAngular(definition);
            var userMessage = OpenAPIPrompts.FewShotCodeSmellPromptUserInput.Replace("{code}", code);

            return new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userMessage)
            };
        }
    }

}