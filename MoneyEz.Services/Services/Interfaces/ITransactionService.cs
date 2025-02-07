using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<BaseResultModel> CreateTransactionForUserAsync(CreateTransactionModel model);
        Task<BaseResultModel> UpdateTransactionForUserAsync(UpdateTransactionModel model);
        Task<BaseResultModel> GetUserTransactionsAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetUserTransactionByIdAsync(Guid id);
        Task<BaseResultModel> RemoveUserTransactionAsync(Guid id);

        Task<BaseResultModel> CreateTransactionForGroupAsync(CreateTransactionModel model);
        Task<BaseResultModel> UpdateTransactionForGroupAsync(UpdateTransactionModel model);
        Task<BaseResultModel> GetGroupTransactionsAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetGroupTransactionByUserIdAsync(Guid id);
        Task<BaseResultModel> RemoveGroupTransactionAsync(Guid id);

        Task<BaseResultModel> ApproveTransactionAsync(Guid id);
        Task<BaseResultModel> RejectTransactionAsync(Guid id);
    }
}
