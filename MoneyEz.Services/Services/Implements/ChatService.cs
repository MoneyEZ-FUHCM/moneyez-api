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

        public ChatService(IUnitOfWork unitOfWork, IExternalApiService externalApiService, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _externalApiService = externalApiService;
            _transactionService = transactionService;
        }

        public async Task<ChatMessageResponse> ProcessMessageAsync(Guid userId, string message)
        {
            try
            {
                // Process message through external API
                var apiResponse = await _externalApiService.ProcessMessageAsync(new ChatMessageRequest
                {
                    Message = message
                });

                if (apiResponse.HttpCode == 400)
                {
                    return new ChatMessageResponse
                    {
                        Message = "Có lỗi trong quá trình xử lí. Thử lại sau."
                    };
                }

                // get user
                var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    // process message (answer) from bot

                    if (apiResponse.Command == "ADD_TRANSACTION" && apiResponse.Data != null)
                    {
                        // find subcategory
                        var subCategories = await _unitOfWork.SubcategoryRepository.GetAllAsync();
                        var subCategory = subCategories.FirstOrDefault(x => x.Name == apiResponse.Data.SubCategoryName);
                        if (subCategory != null)
                        {
                            // Add transaction to database
                            var newTransaction = new CreateTransactionModel
                            {
                                Amount = apiResponse.Data.Amount.Value,
                                SubcategoryId = subCategory.Id,
                                Description = apiResponse.Data.Description,
                                TransactionDate = CommonUtils.GetCurrentTime(),
                            };

                            await _transactionService.CreateTransactionAsync(newTransaction, user.Email);

                            return new ChatMessageResponse
                            {
                                Message = $"Đã thêm thành công {newTransaction.Amount} VNĐ vào mục {subCategory.Name}"
                            };

                        }
                    }
                    else
                    {
                        // TODO: Handle other commands
                    }
                }
                return new ChatMessageResponse
                {
                    Message = "Có lỗi trong quá trình xử lí. Thử lại sau."
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
