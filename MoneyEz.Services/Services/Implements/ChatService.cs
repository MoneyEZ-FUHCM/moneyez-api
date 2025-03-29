using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Repositories.Utils;

namespace MoneyEz.Services.Services.Implements
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalApiService _externalApiService;
        private readonly ITransactionService _transactionService;
        private readonly IChatHistoryService _chatHistoryService;

        public ChatService(IUnitOfWork unitOfWork, IExternalApiService externalApiService, 
            ITransactionService transactionService, IChatHistoryService chatHistoryService)
        {
            _unitOfWork = unitOfWork;
            _externalApiService = externalApiService;
            _transactionService = transactionService;
            _chatHistoryService = chatHistoryService;
        }

        public async Task<ChatMessageResponse> ProcessMessageAsync(Guid userId, string message)
        {
            try
            {
                // get chat history of user
                var chatHistories = await _chatHistoryService.GetChatMessageHistoriesExternalByUser(userId);

                // Process message through external API
                var apiResponse = await _externalApiService.ProcessMessageAsync(new ChatMessageRequest
                {
                    UserId = userId,
                    Message = message,
                    ConversationId = chatHistories.FirstOrDefault().ConversationId,
                    PreviousMessage = chatHistories
                });

                if (apiResponse.IsSuccess == false)
                {
                    return new ChatMessageResponse
                    {
                        Message = "Có lỗi trong quá trình xử lí. Thử lại sau."
                    };
                }

                return new ChatMessageResponse
                {
                    Message = apiResponse.Message,
                };
            }
            catch
            {
                return new ChatMessageResponse
                {
                    Message = "Có lỗi trong quá trình xử lí. Thử lại sau."
                };
            }
        }
    }
}
