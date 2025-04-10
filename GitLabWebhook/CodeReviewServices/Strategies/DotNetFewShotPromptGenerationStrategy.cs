
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{

    public class DotNetFewShotPromptGenerationStrategy : FewShotPromptGenerationStrategy
    {
        public override IEnumerable<Enum> CodeSmellTypes => Enum.GetValues(typeof(DotNetCodeSmellType)).Cast<Enum>();

        public override List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
        {
            var definition = DotNetCodeSmells.GetDefinition((DotNetCodeSmellType)codeSmellType);
            var systemMessage = OpenAPIPrompts.GetPositiveFewShotCodeSmellSystemMessageDotNet(definition);
            var userMessage = OpenAPIPrompts.FewShotCodeSmellPromptUserInput.Replace("{code}", code);

            return new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userMessage)
            };
        }
    }

}