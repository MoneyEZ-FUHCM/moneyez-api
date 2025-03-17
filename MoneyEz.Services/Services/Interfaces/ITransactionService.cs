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
        Task<BaseResultModel> GetAllTransactionsForUserAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId);
        Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model);
        Task<BaseResultModel> UpdateTransactionAsync(UpdateTransactionModel model);
        Task<BaseResultModel> DeleteTransactionAsync(Guid transactionId);
        Task<BaseResultModel> GetAllTransactionsForAdminAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> GetTransactionByGroupIdAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> CreateGroupTransactionAsync(CreateGroupTransactionModel model);
        Task<BaseResultModel> GetGroupTransactionDetailsAsync(Guid transactionId);
        Task<BaseResultModel> UpdateGroupTransactionAsync(UpdateGroupTransactionModel model);
        Task<BaseResultModel> DeleteGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> ApproveGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> RejectGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> CreateGroupTransactionVoteAsync(CreateGroupTransactionVoteModel model);
        Task<BaseResultModel> UpdateGroupTransactionVoteAsync(UpdateGroupTransactionVoteModel model);
        Task<BaseResultModel> DeleteGroupTransactionVoteAsync(Guid voteId);
    }
}
