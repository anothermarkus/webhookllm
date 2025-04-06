using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{
    public interface IPromptGenerationStrategyFactory
    {
        public IPromptGenerationStrategy GetStrategy(StrategyType strategyType);
    }

}