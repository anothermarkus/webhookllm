using System.ClientModel;
using GitLabWebhook.Models;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

// Open API Documentation https://github.com/openai/openai-dotnet/tree/OpenAI_2.1.0
// Documentation https://platform.openai.com/docs/api-reference/introduction
// Cookbook code quality https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions

namespace CodeReviewServices
{
    
    /// <summary>Class level description for OpenAIService.</summary>   
    public class OpenAIService
    {
        private string _apiKey;
        private string _openAiBaseUrl; // Custom Base URL
        private readonly ChatClient _chatClient;
        private List<CodeSmellExampleEmbeddings> _storedCodeSmellExampleEmbeddings;
        private List<CodeReviewCriteriaEmbeddings> _storedReviewCriteriaEmbeddings; 


        /// <summary>
        /// Initializes a new instance of the OpenAIService class.
        ///
        /// This constructor retrieves the OPENAPITOKEN from the environment and sets it as the _apiKey.
        /// It also sets the _openAiBaseUrl to "https://genai-api-dev.dell.com/v1/" which is the Open API
        /// 2.3.0 - Equivalent to GPT-3.5.
        ///
        /// The function then creates a new ApiKeyCredential using the _apiKey.
        /// After that, it creates a new OpenAIClientOptions with the specified _openAiBaseUrl.
        /// Finally, it initializes a new ChatClient with the specified parameters.
        /// </summary>
        public OpenAIService()
        {
            _apiKey = Environment.GetEnvironmentVariable("OPENAPITOKEN") ?? throw new ArgumentNullException("OPENAPITOKEN needs to be set in the environment");
            _openAiBaseUrl = "https://genai-api-dev.dell.com/v1/"; //Open API 2.3.0 - Equivalent to GPT-3.5

            var apiKeyCredential = new ApiKeyCredential(_apiKey);
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(_openAiBaseUrl), // Specify the hostname here
            };
            _chatClient = new ChatClient("mixtral-8x7b-instruct-v01", apiKeyCredential, options);
        }

    
        /// <summary>
        /// General Purpose Method to Asynchronously reviews the given code document based on the specified review criteria.
        /// </summary>
        public async Task<string> GetFeedback(List<ChatMessage> messages)
        {   
            // Request completion from the OpenAI API
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0,  // Set the temperature (controls randomness)
                TopP = 1.0f, // Keeps sampling open
            };

            var result = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            return result.Value.Content[0].Text;
        }

    }

}
