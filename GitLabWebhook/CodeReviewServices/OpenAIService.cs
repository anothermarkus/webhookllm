using System.ClientModel;
using Models;
using OpenAI;
using OpenAI.Chat;

namespace CodeReviewServices
{
    // TODO: Review API Patterns https://github.com/openai/openai-dotnet/tree/OpenAI_2.1.0
    // Documentation https://platform.openai.com/docs/api-reference/introduction
    // Cookbook code quality https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions

    public class OpenAIService
    {
        private string _apiKey;
        private string _openAiBaseUrl; // Custom Base URL
        private readonly ChatClient _chatClient;

        public OpenAIService()
        {
            _apiKey = Environment.GetEnvironmentVariable("OPENAPITOKEN");
            _openAiBaseUrl = "https://genai-api-dev.dell.com/v1/"; //Open API 2.3.0 - Equivalent to GPT-3.5

            var apiKeyCredential = new ApiKeyCredential(_apiKey);
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(_openAiBaseUrl), // Specify the hostname here
            };
            _chatClient = new ChatClient("codellama-13b-instruct", apiKeyCredential, options);
        }

        public async Task<FileDiff> PopulateSuggestion(FileDiff fileDiff)
        {
            var reviewCriteria = new List<string>
            {
                "Check for unused variables",
                "Ensure all functions have docstrings",
                "Check for code duplication",
                "Ensure error handling is implemented",
            };

            var result = await ReviewCodeAsync(fileDiff.Diff, reviewCriteria);

            if (result != null)
            {
                fileDiff.HasSuggestion = true;
                fileDiff.LLMComment = result;
                // TODO parse result as JSON and fetch line number from it
            }

            return fileDiff;
        }

        public async Task<string> ReviewCodeAsync(string codeDocument, List<string> reviewCriteria)
        {
            var criteriaPrompt = string.Join("\n", reviewCriteria);
            var prompt =
                $"Here is the code:\n{codeDocument}\n\nPlease review the code based on the following criteria:\n{criteriaPrompt}\n"
                + "Provide suggestions and feedback for improvements.";

            var messages = new List<ChatMessage> { new UserChatMessage(prompt) };

            // Request completion from the OpenAI API
            var result = await _chatClient.CompleteChatAsync(messages);

            // Return the assistant's feedback
            return result.Value.Content[0].Text;
        }
    }
}
