using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;
using GitLabWebhook.CodeReviewServices.Strategies;

namespace GitLabWebhook.CodeReviewServices.Decorators
{

    public abstract class PromptGenerationStrategyDecorator : IPromptGenerationStrategy
    {
        protected readonly IPromptGenerationStrategy _inner;

        protected PromptGenerationStrategyDecorator(IPromptGenerationStrategy inner)
        {
            _inner = inner;
        }

        public abstract List<ChatMessage> GetMessagesForPrompt(string code);
        }

}
