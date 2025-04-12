// using OpenAI;
// using OpenAI.Chat;
// using GitLabWebhook.Models;
// using GitLabWebhook.CodeReviewServices.Strategies;

// namespace GitLabWebhook.CodeReviewServices.Decorators
// {
//     public class AngularPromptDecorator : PromptGenerationStrategyDecorator
//     {
//         public AngularPromptDecorator(IPromptGenerationStrategy inner) : base(inner) {}

//         public List<ChatMessage> GetMessagesForPrompt(string code, Enum codeSmellType)
//         {
//             var messages = _inner.GetMessagesForPrompt(code);
//             var codeSmellDefinition = AngularCodeSmells.GetDefinition((AngularCodeSmellType)codeSmellType);
//             var systemInput = OpenAPIPrompts.GetPositiveFewShotCodeSmellSystemMessageAngular(codeSmellDefinition);
//             messages.Insert(0, new SystemChatMessage(systemInput));

//            // var assistantInput = OpenAPIPrompts.GetPositiveFewShotCodeAssistantMessageAngular(codeSmellDefinition);
//            // messages.Add(new AssistantChatMessage(assistantInput));

//             return messages;
//         }

//         public override List<ChatMessage> GetMessagesForPrompt(string code)
//         {
//             var messages = _inner.GetMessagesForPrompt(code);
           
//             var systemInput = OpenAPIPrompts.CodeReviewPromptAngular;
//             messages.Insert(0, new SystemChatMessage(systemInput));

//             return messages;
//         }

//         public override IEnumerable<Enum> CodeSmellTypes { get; } = Enum.GetValues(typeof(AngularCodeSmellType)).Cast<Enum>();

//     }
// }
