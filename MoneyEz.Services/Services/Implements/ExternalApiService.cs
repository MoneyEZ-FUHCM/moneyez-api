﻿using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        private readonly ISpendingModelService _spendingModelService;
        private readonly IMapper _mapper;

        public ExternalApiService(HttpClient httpClient, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ITransactionService transactionService,
            IChatHistoryService chatHistoryService,
            IUserSpendingModelService userSpendingModelService,
            IMapper mapper,
            ISpendingModelService spendingModelService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _transactionService = transactionService;
            _chatHistoryService = chatHistoryService;
            _userSpendingModelService = userSpendingModelService;
            _spendingModelService = spendingModelService;
            _mapper = mapper;
            // Configure base URL from settings if needed
            // _httpClient.BaseAddress = new Uri(_configuration["ExternalApi:BaseUrl"]);
        }

        public async Task<BaseResultModel> ExecuteKnownledgeDocumentSerivce(ExternalKnowledgeRequestModel model, PaginationParameter paginationParameter)
        {
            switch (model.Command)
            {
                case "get_all_documents":
                    return await GetAllKnowledgeDocument(paginationParameter);
                default:
                    throw new DefaultException("Invalid command", MessageConstants.INVALID_COMMAND);
            }
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
                case "create_transaction_v2":
                    var dataV2 = model.Data as dynamic;
                    var parsedDataJsonV2 = dataV2?.ToString();
                    var jsonDataV2 = JsonConvert.DeserializeObject<dynamic>(parsedDataJsonV2);
                    var createTransactionModelV2 = new CreateTransactionPythonModelV2
                    {
                        Amount = jsonDataV2?.Amount ?? 0,
                        SubcategoryCode = jsonDataV2?.SubcategoryCode ?? string.Empty,
                        Description = jsonDataV2?.Description ?? string.Empty,
                        UserId = jsonDataV2?.UserId ?? Guid.Empty,
                        TransactionDate = jsonDataV2?.TransactionDate ?? CommonUtils.GetCurrentTime()
                    };
                    // Call service to create transaction
                    return await _transactionService.CreateTransactionPythonServiceV2(createTransactionModelV2);
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
                case "get_spending_models":
                    return await _spendingModelService.GetAllSpendingModelsAsync();
                case "get_transaction_histories_user":
                    // Extract param from query parameter (e.g., user_id=abc)
                    if (string.IsNullOrEmpty(model.Query))
                    {
                        throw new DefaultException("Missing user_id parameter", MessageConstants.MISSING_PARAMETER);
                    }
                    else
                    {
                        // initialize filter
                        var filter = new TransactionFilter
                        {
                            FromDate = null,
                            ToDate = null,
                        };
                        // Parse the query string to get parameters
                        var queryParams = new Dictionary<string, string>();
                        if (model.Query.Contains(","))
                        {
                            // Multiple parameters in query string separated by commas
                            var parameters = model.Query.Split(',');
                            foreach (var param in parameters)
                            {
                                if (param.Contains("="))
                                {
                                    var parts = param.Split('=', 2);
                                    queryParams[parts[0].Trim()] = parts[1].Trim();
                                }
                            }
                        }
                        else if (model.Query.Contains("="))
                        {
                            // Single parameter with key=value format
                            var parts = model.Query.Split('=', 2);
                            queryParams[parts[0].Trim()] = parts[1].Trim();
                        }
                        else
                        {
                            // Assume it's just the userId with no key
                            queryParams["user_id"] = model.Query.Trim();
                        }

                        // Get userId
                        if (!queryParams.TryGetValue("user_id", out string userIdStr) || string.IsNullOrEmpty(userIdStr))
                        {
                            throw new DefaultException("Missing user_id parameter", MessageConstants.MISSING_PARAMETER);
                        }
                        if (!Guid.TryParse(userIdStr, out Guid userId))
                        {
                            throw new DefaultException("Invalid user_id format", MessageConstants.INVALID_PARAMETER_FORMAT);
                        }
                        
                        // Parse from_date if provided
                        if (queryParams.TryGetValue("from_date", out string fromDateStr) && !string.IsNullOrEmpty(fromDateStr))
                        {
                            if (DateTime.TryParse(fromDateStr, out DateTime fromDate))
                            {
                                filter.FromDate = fromDate;
                            }
                            else
                            {
                                throw new DefaultException("Invalid from_date format", MessageConstants.INVALID_PARAMETER_FORMAT);
                            }
                        }
                        
                        // Parse to_date if provided
                        if (queryParams.TryGetValue("to_date", out string toDateStr) && !string.IsNullOrEmpty(toDateStr))
                        {
                            if (DateTime.TryParse(toDateStr, out DateTime toDate))
                            {
                                filter.ToDate = toDate;
                            }
                            else
                            {
                                throw new DefaultException("Invalid to_date format", MessageConstants.INVALID_PARAMETER_FORMAT);
                            }
                        }
                        
                        // Call service to get transaction histories for the user with date filters
                        return await _transactionService.GetTransactionHistorySendToPythons(userId, filter);
                    }
                case "get_user_spending_model":
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
                        return await _userSpendingModelService.GetCurrentSpendingModelByUserIdAsync(userId);
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

                //var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/receive_message", new
                //{
                //    data = jsonString
                //});

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

        public async Task<BaseResultModel> GetAllKnowledgeDocument(PaginationParameter paginationParameter)
        {
            var response = await _httpClient.GetAsync("http://178.128.118.171:8888/api/knowledge/knowledge/documents");
            if (response.IsSuccessStatusCode)
            {
                // document from qdrant db - python service
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonData = JsonConvert.DeserializeObject<BaseResultModel>(jsonString);
                var rawDocuments = JsonConvert.DeserializeObject<List<ResponseKnowledgeModel>>(jsonData.Data.ToString());

                var documents = _mapper.Map<List<KnowledgeModel>>(rawDocuments);
                var itemCount = documents.Count;
                var paginatedDocuments = documents
                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                    .Take(paginationParameter.PageSize)
                    .ToList();

                var paging = new Pagination<KnowledgeModel>(paginatedDocuments, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
                var result = PaginationHelper.GetPaginationResult(paging, paginatedDocuments);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = result
                };
            }
            throw new DefaultException("Failed to fetch documents", "ErrorFetchDocument");
        }

        public async Task<RecomendModelResponse> SuggestionSpendingModelSerivce(List<QuestionAnswerPair> answerPairs)
        {
            try
            {
                // Clear and set new headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-External-Secret", "thisIsSerectKeyPythonService");

                var jsonString = JsonConvert.SerializeObject(answerPairs);
                Console.WriteLine("JSON payload: " + jsonString);

                var response = await _httpClient.PostAsJsonAsync("http://178.128.118.171:8888/api/suggestion", new
                {
                    data = jsonString
                });

                //var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/suggestion", new
                //{
                //    data = jsonString
                //});

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BaseResultModel>();

                    if (result != null)
                    {
                        // Extract the first text content from the message
                        var parsedDataJson = result.Data?.ToString();

                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            }
                        };

                        var jsonData = JsonConvert.DeserializeObject<RecomendModelResponse>(parsedDataJson, settings);

                        return jsonData;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<BaseResultModel> SuggestionSpendingModelSerivceTest(List<QuestionAnswerPair> answerPairs)
        {
            try
            {
                // Clear and set new headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-External-Secret", "thisIsSerectKeyPythonService");

                var jsonString = JsonConvert.SerializeObject(answerPairs);
                Console.WriteLine("JSON payload: " + jsonString);

                var response = await _httpClient.PostAsJsonAsync("http://178.128.118.171:8888/api/suggestion", new
                {
                    data = jsonString
                });

                //var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/suggestion", new
                //{
                //    data = jsonString
                //});

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BaseResultModel>();

                    if (result != null)
                    {
                        // Extract the first text content from the message
                        var parsedDataJson = result.Data?.ToString();

                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            }
                        };

                        var jsonData = JsonConvert.DeserializeObject<RecomendModelResponse>(parsedDataJson, settings);

                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status200OK,
                            Data = jsonData
                        };
                    }
                }
                return new BaseResultModel
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Failed to fetch recommendation"
                };
            }
            catch
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Failed to fetch recommendation"
                };
            }
        }
    }
}
