using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;
using GitLabWebhook.CodeReviewServices.Strategies;

namespace GitLabWebhook.CodeReviewServices.Decorators
{
    public class DotNetPromptDecorator : PromptGenerationStrategyDecorator
    {
        public DotNetPromptDecorator(IPromptGenerationStrategy inner) : base(inner) {}

        public override List<ChatMessage> GetMessagesForPrompt(string code)
        {
            var messages = _inner.GetMessagesForPrompt(code);

            var systemInput = OpenAPIPrompts.FewShotCodeSmellSystemMessageDotNet;
            messages.Insert(0, new SystemChatMessage(systemInput));

            return messages;
        }
    }
}