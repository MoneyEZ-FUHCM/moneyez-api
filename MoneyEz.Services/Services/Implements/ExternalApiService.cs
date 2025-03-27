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
        private readonly IChatHistoryService _chatHistoryService;

        public ExternalApiService(HttpClient httpClient, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ITransactionService transactionService,
            IChatHistoryService chatHistoryService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _transactionService = transactionService;
            _chatHistoryService = chatHistoryService;
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
                case "get_chat_messages":
                    // Extract userId from query parameter (e.g., user_id=abc)
                    if (string.IsNullOrEmpty(model.Query))
                    {
                        throw new DefaultException("Missing user_id parameter", MessageConstants.MISSING_PARAMETER);
                    }

                    // Parse the query string to get the userId
                    string userIdStr = model.Query;
                    if (model.Query.Contains("user_id="))
                    {
                        userIdStr = model.Query.Split('=')[1];
                    }

                    if (!Guid.TryParse(userIdStr, out Guid userId))
                    {
                        throw new DefaultException("Invalid user_id format", MessageConstants.INVALID_PARAMETER_FORMAT);
                    }

                    // Call service to get chat messages for the user
                    return await _chatHistoryService.GetChatMessageHistoriesExternalByUser(userId);
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
