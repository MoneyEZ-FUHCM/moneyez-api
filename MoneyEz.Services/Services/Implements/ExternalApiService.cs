using System.Net.Http.Json;
using System.Net.WebSockets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoneyEz.Services.Services.Implements
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionService _transactionService;
        private readonly IChatHistoryService _chatHistoryService;
        private readonly IUserSpendingModelService _userSpendingModelService;

        public ExternalApiService(HttpClient httpClient, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ITransactionService transactionService,
            IChatHistoryService chatHistoryService,
            IUserSpendingModelService userSpendingModelService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _transactionService = transactionService;
            _chatHistoryService = chatHistoryService;
            _userSpendingModelService = userSpendingModelService;
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
                    var parsedDataJson = data?.ToString();
                    var jsonData = JsonConvert.DeserializeObject<dynamic>(parsedDataJson);
                    var createTransactionModel = new CreateTransactionPythonModel
                    {
                        Amount = jsonData?.Amount ?? 0,
                        SubcategoryCode = jsonData?.SubcategoryCode ?? string.Empty,
                        Description = jsonData?.Description ?? string.Empty,
                        UserId = jsonData?.UserId ?? Guid.Empty
                    };
                    // Call service to create transaction
                    return await _transactionService.CreateTransactionPythonService(createTransactionModel);
                case "get_chat_messages":
                    // Extract userId from query parameter (e.g., user_id=abc)
                    if (string.IsNullOrEmpty(model.Query))
                    {
                        throw new DefaultException("Missing user_id parameter", MessageConstants.MISSING_PARAMETER);
                    } 
                    else
                    {
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
                        var messages = await _chatHistoryService.GetChatMessageHistoriesExternalByUser(userId);
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status200OK,
                            Data = messages
                        };
                    }
                        
                case "get_subcategories":
                    // Extract userId from query parameter (e.g., user_id=abc)
                    if (string.IsNullOrEmpty(model.Query))
                    {
                        throw new DefaultException("Missing user_id parameter", MessageConstants.MISSING_PARAMETER);
                    }
                    else
                    {
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

                        // Call service to get subcategories for the user
                        return await _userSpendingModelService.GetSubCategoriesCurrentSpendingModelByUserIdAsync(userId);
                    }
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
                // Clear and set new headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-External-Secret", "thisIsSerectKeyPythonService");

                var jsonString = JsonConvert.SerializeObject(request);
                Console.WriteLine("JSON payload: " + jsonString);

                var response = await _httpClient.PostAsJsonAsync("http://178.128.118.171:8888/api/receive_message", new
                {
                    data = jsonString
                });
                //var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/receive_message", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BaseResultModel>();

                    if (result != null)
                    {
                        // Extract the first text content from the message
                        var parsedDataJson = result.Data?.ToString();
                        var jsonData = JsonConvert.DeserializeObject<ChatMessageResponseModel>(parsedDataJson);

                        return new ChatMessageExternalResponse
                        {
                            IsSuccess = true,
                            Message = jsonData.Message.Content.First().Text,
                        };
                    }

                    return new ChatMessageExternalResponse
                    {
                        IsSuccess = true,
                        Message = "Empty response"
                    };
                }

                return new ChatMessageExternalResponse
                {
                    IsSuccess = false,
                    Message = $"API Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageExternalResponse
                {
                    IsSuccess = false,
                    Message = $"Service Error: {ex.Message}"
                };
            }
        }
    }
}
