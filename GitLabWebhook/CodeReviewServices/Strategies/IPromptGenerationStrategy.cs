using OpenAI;
using OpenAI.Chat;

 namespace GitLabWebhook.CodeReviewServices.Strategies{
 
    public interface IPromptGenerationStrategy
    {
        public List<ChatMessage> GetMessagesForPrompt(String request);
    }

}
    