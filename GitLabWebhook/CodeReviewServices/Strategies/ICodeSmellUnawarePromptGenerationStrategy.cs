using System;
using System.Collections.Generic;
using OpenAI;
using OpenAI.Chat;
using GitLabWebhook.Models;

namespace GitLabWebhook.CodeReviewServices.Strategies
{

    public interface ICodeSmellUnawarePromptGenerationStrategy 
    {
        public List<ChatMessage> GetMessagesForPrompt(string code);
    
    }
}