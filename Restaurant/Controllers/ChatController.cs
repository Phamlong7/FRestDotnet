using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace Restaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        // Hardcoded Azure OpenAI settings
        private readonly string _endpoint = "https://frestai3.openai.azure.com/"; // Replace with your actual endpoint
        private readonly string _apiKey = "1ad3113d0f104022a08c5e9131278429"; // Replace with your actual API key
        private readonly string _deploymentName = "FRestAI3"; // Replace with your actual deployment name

        private readonly AzureOpenAIClient _client;

        public ChatController()
        {
            _client = new AzureOpenAIClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
        }

        [HttpPost("stream")]
        public async Task StreamChatAsync([FromBody] ChatRequest request)
        {
            var chatClient = _client.GetChatClient(_deploymentName);

            var options = new ChatCompletionOptions();

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateUserMessage(request.Message)
            };

            Response.ContentType = "text/event-stream";
            await Response.StartAsync();

            StringBuilder citationBuilder = new StringBuilder();

            await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, options))
            {
                if (update.ContentUpdate != null)
                {
                    foreach (var contentPart in update.ContentUpdate)
                    {
                        await Response.WriteAsync($"data: {contentPart.Text}\n\n");
                        await Response.Body.FlushAsync();
                    }
                }
            }

            if (citationBuilder.Length > 0)
            {
                await Response.WriteAsync($"data: \n\nCitations:\n{citationBuilder}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
