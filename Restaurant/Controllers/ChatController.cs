using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.OpenAI;
using static System.Environment;
using OpenAI.Chat;

namespace Restaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AzureOpenAIClient _azureClient;
        private readonly ChatClient _chatClient;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IConfiguration configuration, ILogger<ChatController> logger)
        {
            _logger = logger;
            var endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var apiKey = GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

            _logger.LogInformation($"Endpoint: {endpoint}");
            _logger.LogInformation($"API Key: {apiKey}"); // Ensure not to log sensitive information in production!
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI endpoint and API key must be set in environment variables.");
            }

            _azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _chatClient = _azureClient.GetChatClient("gpt-35-turbo"); // Use your custom deployment name
        }

        [HttpPost("stream")]
        public async Task StreamChatAsync([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                Response.StatusCode = 400; // Bad Request
                await Response.WriteAsync("Message cannot be null or empty.");
                return;
            }
            Response.ContentType = "text/event-stream";
            await Response.StartAsync();
            try
            {
                var chatUpdates = _chatClient.CompleteChatStreamingAsync(
                    new List<ChatMessage>
                    {
                new UserChatMessage(request.Message)
                    });

                await foreach (var chatUpdate in chatUpdates)
                {
                    if (chatUpdate.Role.HasValue)
                    {
                        await Response.WriteAsync($"data: {chatUpdate.Role}:\n\n");
                        await Response.Body.FlushAsync();
                    }

                    foreach (var contentPart in chatUpdate.ContentUpdate)
                    {
                        var lines = contentPart.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        foreach (var line in lines)
                        {
                            await Response.WriteAsync($"data: {line}\n\n");
                            await Response.Body.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while streaming chat.");
                await Response.WriteAsync($"data: Error: {ex.Message}\n\n");
            }
            finally
            {
                await Response.Body.FlushAsync();
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
