using System.ClientModel;
using Models;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

// Open API Documentation https://github.com/openai/openai-dotnet/tree/OpenAI_2.1.0
// Documentation https://platform.openai.com/docs/api-reference/introduction
// Cookbook code quality https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions

//private readonly List<string> _reviewCriteria = new List<string>
//{
//    "You are an Enterprise Code Assistant ensuring the code follows best practices and I am providing you with a JSON array of file changes for a merge request (MR). " +
//    "Each item in the array represents a file change in the MR.",

//    "Review each file change thoroughly for these code standards:",

//     // Code Style               
//    "Ensure adherence to DRY (Don't Repeat Yourself) principle by avoiding code duplication and reusing code effectively.",
//    "Maintain cyclomatic complexity under 10; break down complex methods into smaller, manageable ones.",
//    "Avoid deep nesting; use early returns for cleaner logic flow.",
//    "Use null-conditional operators (?.) and null-coalescing operators (??) for safe null value access.",
//    "Implement guard clauses to handle null or invalid inputs early in methods.",

//    // Memory Management
//    "Always use 'using' statements for disposable objects to ensure automatic resource disposal.",
//    "Minimize memory leaks by unsubscribing from events when no longer needed.",
//    "Dispose of unmanaged resources properly and be mindful of large object retention in memory.",
//    "Avoid unnecessary object creation; use weak references or caching where applicable.",

//    // Error Handling
//    "Use try-catch blocks to handle exceptions; catch specific exceptions, not generic ones.",
//    "Always use 'finally' for cleanup operations to release resources.",
//    "Avoid silent failures; log exceptions for troubleshooting and debugging.",
//    "Throw custom exceptions only for business logic errors, not for regular control flow.",
//    "Don't use exceptions for control flow; use conditional checks instead.",

//    // Thread Handling & Async/Await
//    "Use async/await for asynchronous programming; avoid manually managing threads.",
//    "Use ConfigureAwait(false) to avoid deadlocks in non-UI thread operations.",
//    "Avoid blocking async calls (e.g., don't use Result or Wait()).",
//    "Ensure thread safety by using locks or thread-safe collections when accessing shared resources.",
//    "Use CancellationToken for graceful cancellation of long-running async operations.",
//    "Avoid using Thread.Sleep() in async code; prefer Task.Delay() for non-blocking waits.",

//    "For each file change, please provide feedback in the following JSON format:",

//    "- `FileName`: The name of the file being reviewed. This should be provided as is.",
//    "- `LLMComment`: A comment or feedback about the file change. If no comment is necessary, leave it as an empty string. If you suggest a change or improvement, provide it here.",
//    "- `LineForComment`: The line number where you are suggesting a change **within the context of the diff**. If there is no specific line to comment on, use 0.",

//    "Ensure that the `LineForComment` refers to a specific line in the diff where you have feedback. If there is no suggestion, set `LineForComment` to 0.",

//    "Please note: `string.IsNullOrWhiteSpace` is an extension method, so you do not need to manually check for null. It already handles null checks internally. Make sure not to suggest adding any additional null checks where `string.IsNullOrWhiteSpace` is used.",

//    "The code should be self-documenting and should not require additional comments or clarification about its behavior.",

//    "Please respond with a JSON array only. The structure should be [ `FileName`, `LLMComment`, `LineForComment` ]."
//};

// LLM is having trouble with line numbers...


// gleaned from: https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions
//private readonly List<string> _reviewCriteria = new List<string>
//{
//    "You are an Enterprise Code Assistant. Review each code snippet below for its adherence to the following categories",
//    "1) Code Style & Formatting",
//    "2) Security & Compliance",
//    "3) Error Handling & Logging",
//    "4) Readability & Maintainability",
//    "5) Performance & Scalability",
//    "6) Testing & Quality Assurance",
//    "7) Documentation & Version Control",
//    "8) Accessibility & Internationalization",
//    "Create a table and assign a rating of 'extraordinary', 'acceptable', or 'poor' for each category. Return a markdown table titled 'Enterprise Standards' with rows for each category and columns for 'Category' and 'Rating'",
//    "Here are the changed file contents to analyze:"
//};


namespace CodeReviewServices
{
    
    /// <summary>Class level description for OpenAIService.</summary>   
    public class OpenAIService
    {
        private string _apiKey;
        private string _openAiBaseUrl; // Custom Base URL
        private readonly ChatClient _chatClient;

      
        private readonly List<string> _reviewCriteria = new List<string>
            {
                "You are an Enterprise Code Assistant ensuring the code follows best practices and I am providing you with a JSON array of file changes for a merge request (MR). " +
                "Each item in the array represents a file change in the MR.",

                "Review each file change thoroughly for these code standards:",

                 // Code Style
                "CODE STYLE:",
                "Ensure adherence to DRY (Don't Repeat Yourself) principle by avoiding code duplication and reusing code effectively.",
                "Maintain cyclomatic complexity under 10; break down complex methods into smaller, manageable ones.",
                "Avoid deep nesting; use early returns for cleaner logic flow.",
                "Use null-conditional operators (?.) and null-coalescing operators (??) for safe null value access.",
                "Implement guard clauses to handle null or invalid inputs early in methods.",

                // Memory Management
                "MEMORY MANAGEMENT:",
                "Always use 'using' statements for disposable objects to ensure automatic resource disposal.",
                "Minimize memory leaks by unsubscribing from events when no longer needed.",
                "Dispose of unmanaged resources properly and be mindful of large object retention in memory.",
                "Avoid unnecessary object creation; use weak references or caching where applicable.",

                // Error Handling
                "ERROR HANDLING:",
                "Use try-catch blocks to handle exceptions; catch specific exceptions, not generic ones.",
                "Always use 'finally' for cleanup operations to release resources.",
                "Avoid silent failures; log exceptions for troubleshooting and debugging.",
                "Throw custom exceptions only for business logic errors, not for regular control flow.",
                "Don't use exceptions for control flow; use conditional checks instead.",

                // Thread Handling & Async/Await
                "THREAD HANDLING:",
                "Use async/await for asynchronous programming; avoid manually managing threads.",
                "Use ConfigureAwait(false) to avoid deadlocks in non-UI thread operations.",
                "Avoid blocking async calls (e.g., don't use Result or Wait()).",
                "Ensure thread safety by using locks or thread-safe collections when accessing shared resources.",
                "Use CancellationToken for graceful cancellation of long-running async operations.",
                "Avoid using Thread.Sleep() in async code; prefer Task.Delay() for non-blocking waits.",

                "For each file change, please provide feedback in the following format:",

                "FileName: The name of the file being reviewed. ",
                "Comment: A comment or feedback about the file chang.  If no comment is necessary, leave it as an empty string. If you suggest a change or improvement, provide it here along with the line number.",

                "Please note: `string.IsNullOrWhiteSpace` is an extension method, so you do not need to manually check for null. It already handles null checks internally. Make sure not to suggest adding any additional null checks where `string.IsNullOrWhiteSpace` is used.",

                "The code should be self-documenting and should not require additional comments or clarification about its behavior.",
                "Please return everything in MarkDown format along with an overall grade: Great, Satisfactory or Poor"
           };

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
            var criteriaPrompt = string.Join("\n", _reviewCriteria);
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
        public async Task<string> ReviewCodeWithEmbeddingsAsync(string code)
        {

            // TODO Move embeddings bootstrapping to  IHostedService
            //
            // builder.Services.AddSingleton<PreloadingService>();
            // builder.Services.AddHostedService(sp => sp.GetRequiredService<PreloadingService>());
            // 
            // Use it 
            //   public SomeOtherService(PreloadingService preloadingService)
            // {
            //     _preloadingService = preloadingService;
            // }   

            //string newCodeSnippet = "double circleArea = 3.14 * radius * radius;";

            var (relevantRules, relevantReviews) = await RetrieveRelevantContext(code);

            string prompt = $"Review the following code:\n\n{code}\n\n"
                    + $"Follow these rules:\n- {string.Join("\n- ", relevantRules)}\n\n"
                    + $"Consider these past review comments:\n- {string.Join("\n- ", relevantReviews)}\n\n"
                    + "Provide a detailed review.";



            var messages = new List<ChatMessage> { new UserChatMessage(prompt) };

            // Request completion from the OpenAI API
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.7f,  // Set the temperature (controls randomness)
            };

            var result = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            return string.Join(Environment.NewLine, result.Value.Content[0].Text.Split(new[] { '\r', '\n' }).Skip(2));

        }

     

        private async Task<List<ReadOnlyMemory<float>>> GenerateEmbeddings(string[] texts)
        {

            // TODO Move embeddings bootstrapping to  IHostedService
            var  _apiKey = Environment.GetEnvironmentVariable("OPENAPITOKEN") ?? throw new ArgumentNullException("OPENAPITOKEN needs to be set in the environment");
            var _openAiBaseUrl = "https://genai-api-dev.dell.com/v1/"; //Open API 2.3.0 - Equivalent to GPT-3.5

            var apiKeyCredential = new ApiKeyCredential(_apiKey);
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(_openAiBaseUrl), // Specify the hostname here
            };

            EmbeddingClient _embeddingClient = new EmbeddingClient("text-embedding-3-small", apiKeyCredential, options);

            var embeddings = new List<ReadOnlyMemory<float>>();

            foreach (var text in texts)
            {
                OpenAIEmbedding embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
                embeddings.Add(embedding.ToFloats());
            }

            return embeddings;
        }

        private List<string> GetTopMatches(ReadOnlyMemory<float> queryEmbedding, List<ReadOnlyMemory<float>> storedEmbeddings, string[] texts, int topK)
        {
            var scores = storedEmbeddings.Select((embedding, index) => new
            {
                Text = texts[index],
                Score = CosineSimilarity(queryEmbedding.Span, embedding.Span)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Text)
            .ToList();

            return scores;
        }

        private float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            float dotProduct = 0, normA = 0, normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
        }


        private async Task<(List<string> relevantRules, List<string> relevantReviews)> RetrieveRelevantContext(string newCodeSnippet)
        {
            // Define Code Review Rules
            string[] codeReviewRules = {
                    "Avoid using magic numbers.",
                    "Ensure proper exception handling.",
                    "Use meaningful variable names."
                };

            // Define Past Reviews
            string[] pastReviews = {
                    "Use a constant instead of '3.14' for Pi.",
                    "Wrap file operations in a try-catch block.",
                    "Rename 'x' to 'customerId' for clarity."
                };

            // Step 1: Generate Embeddings
            var ruleEmbeddings = await GenerateEmbeddings(codeReviewRules);
            var reviewEmbeddings = await GenerateEmbeddings(pastReviews);
            var newCodeEmbedding = (await GenerateEmbeddings(new[] { newCodeSnippet })).First();

            
            // Step 2: Find most relevant rules and reviews using Cosine Similarity 
            var relevantRules = GetTopMatches(newCodeEmbedding, ruleEmbeddings, codeReviewRules, 3);
            var relevantReviews = GetTopMatches(newCodeEmbedding, reviewEmbeddings, pastReviews, 3);

            return (relevantRules, relevantReviews);
        }

    }
}
