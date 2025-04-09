 
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

 namespace GitLabWebhook.CodeReviewServices.Strategies
 {
 
    public class FewShotPromptGenerationStrategy : IPromptGenerationStrategy
    {
        public List<ChatMessage> GetMessagesForPrompt(String code){

            //string systemInput = "TO BE REPLACED WITH DECORATOR";
            //string assistantInput = OpenAPIPrompts.FewShotCodeSmellAssistantMessage;
            string userInput = OpenAPIPrompts.FewShotCodeSmellPromptUserInput.Replace("{code}", code);

            return new List<ChatMessage>
            {              
               // new SystemChatMessage (  systemInput ),
                // TODO Fix and figure out new AssistantChatMessage (  "Smell: Selector Duplication\nExplanation: Duplicate UI selectors used in conditions.\nSuggestion: Use *ngFor." ),
                new UserChatMessage ( userInput )
            };
        }
    }

}
    