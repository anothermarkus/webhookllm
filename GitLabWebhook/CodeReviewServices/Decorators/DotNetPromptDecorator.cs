using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;
using GitLabWebhook.CodeReviewServices.Strategies;

namespace GitLabWebhook.CodeReviewServices.Decorators
{
    public class DotNetPromptDecorator : PromptGenerationStrategyDecorator, ICodeSmellAwarePromptGenerationStrategy
    {
        public DotNetPromptDecorator(IPromptGenerationStrategy inner) : base(inner) {}
        
        public List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
        {
            var messages = _inner.GetMessagesForPrompt(code);
            var codeSmellDefinition = DotNetCodeSmells.GetDefinition((DotNetCodeSmellType)codeSmellType);
            var systemInput = OpenAPIPrompts.GetPositiveFewShotCodeSmellSystemMessageDotNet(codeSmellDefinition);
            messages.Insert(0, new SystemChatMessage(systemInput));
            return messages;
        }
        
        public override List<ChatMessage> GetMessagesForPrompt(string code) => throw new NotSupportedException("Use the overload with codeSmellType.");

        public IEnumerable<Enum> CodeSmellTypes { get; } = Enum.GetValues(typeof(DotNetCodeSmellType)).Cast<Enum>();

    }
}