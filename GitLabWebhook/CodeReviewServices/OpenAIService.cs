using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeReviewServices{


    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly string _openAiBaseUrl;  // Custom Base URL

        public OpenAIService()
        {
            _apiKey = "TODO";  // TODO fetch from ENV
            _openAiBaseUrl = "TODO"; // Use default URL or custom URL
        }

         // Method to generate embeddings
        public async Task<ReadOnlyMemory<float>> GetEmbedding(string text)
        {
            // Initialize EmbeddingClient with model name and API key
            EmbeddingClient embeddingClient = new EmbeddingClient("text-embedding-ada-002", _apiKey);

            // Generate embedding for the input text
            OpenAIEmbedding embedding = embeddingClient.GenerateEmbedding(text);

            // Convert embedding to a readable format
            ReadOnlyMemory<float> vector = embedding.ToFloats();

            return vector;
        }

        // Method to generate a completion (text suggestion)
        public async Task<string> GetCompletion(string prompt)
        {
            // Initialize ChatClient with the model name and API key
            var chatClient = new ChatClient("gpt-4", _apiKey);

            // Create a message list for the chat model
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            // Request completion from the chat model
            ChatCompletion completion = chatClient.CompleteChat(messages);

            // Return the first completion result
            return completion.Content[0].Text;
        }

        public string GetReviewGuideline()
        {
            return @"You are a senior developer who reviews C# code. Follow these best practices:
            1. Ensure proper naming conventions are followed.
            2. Check for code readability and clarity.
            3. Look for proper exception handling.
            4. Ensure performance optimization where necessary.
            5. Verify proper commenting and documentation.
            6. Ensure adherence to SOLID principles and design patterns.
            7. Validate unit tests and test coverage.
            8. Ensure secure coding practices.";
        }




    }

}