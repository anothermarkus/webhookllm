using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{
    public class PromptGenerationStrategyFactory : IPromptGenerationStrategyFactory
    {

        private readonly Dictionary<string, IPromptGenerationStrategy> _strategies;

        public PromptGenerationStrategyFactory(IEnumerable<IPromptGenerationStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.GetType().Name.Replace("PromptGenerationStrategy", "").ToLower());
        }


        public IPromptGenerationStrategy GetStrategy(StrategyType strategyType)
        {
            _strategies.TryGetValue(strategyType.ToString().ToLower(), out var strategy);
             return strategy;
        }
    }

}