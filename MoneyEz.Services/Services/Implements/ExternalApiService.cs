using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ExternalApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            // Configure base URL from settings if needed
            // _httpClient.BaseAddress = new Uri(_configuration["ExternalApi:BaseUrl"]);
        }

        public async Task<ChatMessageExternalResponse> ProcessMessageAsync(ChatMessageRequest request)
        {
            try
            {
                // TODO: Replace with your actual API endpoint
                var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatMessageExternalResponse>();
                    return result ?? new ChatMessageExternalResponse { HttpCode = 200, Message = "Empty response" };
                }

                return new ChatMessageExternalResponse
                {
                    HttpCode = 400,
                    Message = $"API Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageExternalResponse
                {
                    HttpCode = 400,
                    Message = $"Service Error: {ex.Message}"
                };
            }
        }
    }
}
