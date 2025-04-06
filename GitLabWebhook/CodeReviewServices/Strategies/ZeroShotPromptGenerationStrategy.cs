 
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

 namespace GitLabWebhook.CodeReviewServices.Strategies
 {
 
    public class ZeroShotPromptGenerationStrategy : IPromptGenerationStrategy
    {
        public List<ChatMessage> GetMessagesForPrompt(String code){

            var criteriaPrompt = string.Join("\n", OpenAPIPrompts.EnterpriseCodeStandardsCriteria);
            var prompt =
                $"Please review the code based on the following criteria:\n{criteriaPrompt}\n"
                + "Provide suggestions and feedback for improvements.";

            var messages = new List<ChatMessage> { new UserChatMessage(prompt) };

            return new List<ChatMessage>
            {
                new SystemChatMessage ( prompt ),
                new UserChatMessage ( $"Here is the code:\n{code}\n\n" )
            };

        }
    }

}
    