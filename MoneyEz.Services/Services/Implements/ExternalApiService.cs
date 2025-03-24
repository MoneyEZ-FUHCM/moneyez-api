using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionService _transactionService;

        public ExternalApiService(HttpClient httpClient, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ITransactionService transactionService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _transactionService = transactionService;
            // Configure base URL from settings if needed
            // _httpClient.BaseAddress = new Uri(_configuration["ExternalApi:BaseUrl"]);
        }

        public async Task<BaseResultModel> ExecuteReceiveExternalService(ExternalReciveRequestModel model)
        {
            // validate webhook
            var secretKey = _httpContextAccessor.HttpContext?.Request.Headers["X-External-Secret"].ToString();

            if (string.IsNullOrEmpty(secretKey) || secretKey != "thisIsSerectKeyPythonService")
            {
                throw new DefaultException("Invalid external secret key", MessageConstants.INVALID_WEBHOOK_SECRET);
            }

            switch (model.Command)
            {
                case "create_transaction":
                    var data = model.Data as dynamic;
                    var createTransactionModel = new CreateTransactionPythonModel
                    {
                        Amount = data?.Amount ?? 0,
                        SubcategoryCode = data?.SubcategoryCode ?? string.Empty,
                        Description = data?.Description ?? string.Empty,
                        UserId = data?.UserId ?? Guid.Empty
                    };
                    // Call service to create transaction
                    return await _transactionService.CreateTransactionPythonService(createTransactionModel);
                default:
                    throw new DefaultException("Invalid command", MessageConstants.INVALID_COMMAND);
            }
        }

        public Task<BaseResultModel> ExecuteSendExternalService(ExternalSendRequestModel model)
        {
            throw new NotImplementedException();
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
