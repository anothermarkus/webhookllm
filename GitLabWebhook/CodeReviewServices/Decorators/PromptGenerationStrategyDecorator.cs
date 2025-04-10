// using OpenAI;
// using OpenAI.Chat;
// using GitLabWebhook.Models;
// using GitLabWebhook.CodeReviewServices.Strategies;

// namespace GitLabWebhook.CodeReviewServices.Decorators
// {

//     public abstract class PromptGenerationStrategyDecorator : IPromptGenerationStrategy, ICodeSmellAwarePromptGenerationStrategy
//     {
//         protected readonly IPromptGenerationStrategy _inner;

//         protected PromptGenerationStrategyDecorator(IPromptGenerationStrategy inner)
//         {
//             _inner = inner;
//         }

//         public abstract List<ChatMessage> GetMessagesForPrompt(string code);

//         public virtual IEnumerable<Enum> CodeSmellTypes =>
//             (_inner as ICodeSmellAwarePromptGenerationStrategy)?.CodeSmellTypes ?? Enumerable.Empty<Enum>();

//         public virtual List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
//         {
//             if (_inner is ICodeSmellAwarePromptGenerationStrategy smellAware)
//             {
//                 return smellAware.GetMessagesForPrompt(code, codeSmellType);
//             }

//             throw new NotSupportedException("This strategy does not support code smell prompts.");
//         }
//     }

// }


