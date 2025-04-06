using System.ClientModel;
using GitLabWebhook.models;
using Models;
using OpenAI;
using OpenAI.Chat;

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
            _chatClient = new ChatClient("codellama-13b-instruct", apiKeyCredential, options);


        }

        /// <summary>
        /// Populates the suggestion for a given file.
        /// </summary>
        /// <param name="fileDiff">The file for which the suggestion needs to be populated.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the updated FileDiff.</returns>
        public async Task<FileDiff> PopulateSuggestion(FileDiff fileDiff)
        {
            var reviewCriteria = new List<string>
            {
                "Check for unused variables",
                "Ensure all functions have docstrings",
                "Check for code duplication",
                "Ensure error handling is implemented",
            };

            var result = await ReviewCodeAsync(fileDiff.Diff);

            if (result != null)
            {
                fileDiff.HasSuggestion = true;
                fileDiff.LLMComment = result;
                // TODO parse result as JSON and fetch line number from it
            }

            return fileDiff;
        }

        /// <summary>
        /// Asynchronously reviews the given code document based on the specified review criteria.
        /// </summary>
        /// <param name="codeDocument">The code document to review.</param>
        /// <returns>A task that represents the completion of the asynchronous operation. The task result contains the review result as a string.</returns>
        public async Task<string> ReviewCodeAsync(string codeDocument)
        {
            var criteriaPrompt = string.Join("\n", OpenAPIPrompts.EnterpriseCodeStandardsCriteria);
            var prompt =
                $"Here is the code:\n{codeDocument}\n\nPlease review the code based on the following criteria:\n{criteriaPrompt}\n"
                + "Provide suggestions and feedback for improvements.";

            var messages = new List<ChatMessage> { new UserChatMessage(prompt) };

            // Request completion from the OpenAI API
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.7f,  // Set the temperature (controls randomness)
            };

            var result = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            // Omitting the first line of OpenAI generated nonsense 
            var response = string.Join(Environment.NewLine, result.Value.Content[0].Text.Split(new[] { '\r', '\n' }).Skip(2));

            return response;
        }


        /// <summary>
        /// Asynchronously reviews the given code document based on the specified review criteria.
        /// </summary>
        /// <param name="code">The code document to review.</param>
        /// <returns>A task that represents the completion of the asynchronous operation. The task result contains the review result as a string.</returns>
        public async Task<string> AnalyzeCodeSmellsUsingFewShotAsync(string code)
        {   
            string systemInput = OpenAPIPrompts.FewShotCodeSmellSystemMessageAngular;
            //string assistantInput = OpenAPIPrompts.FewShotCodeSmellAssistantMessage;
            string userInput = OpenAPIPrompts.FewShotCodeSmellPromptUserInput.Replace("{{CODE_SNIPPET_HERE}}", code);

            var messages = new List<ChatMessage> 
            { 
                new SystemChatMessage(systemInput),
               // new AssistantChatMessage(assistantInput),
                new UserChatMessage(userInput)   
            };

            //TODO Add guiding examples (positive (smell found), negative (smell not found), multiple smells found)

            // Request completion from the OpenAI API
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.1f,  // Set the temperature (controls randomness)
            };

            var result = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            return result.Value.Content[0].Text;
        }





    }

}
