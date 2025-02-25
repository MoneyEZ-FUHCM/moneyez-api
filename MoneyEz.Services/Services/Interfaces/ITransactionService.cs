using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<BaseResultModel> GetAllTransactionsForUserAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId);
        Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model);
        Task<BaseResultModel> UpdateTransactionAsync(UpdateTransactionModel model);
        Task<BaseResultModel> DeleteTransactionAsync(Guid transactionId);
        Task<BaseResultModel> GetAllTransactionsForAdminAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetTransactionByGroupIdAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);

    }
}
