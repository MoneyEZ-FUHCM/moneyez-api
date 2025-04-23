using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.BusinessModels.TransactionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGroupTransactionService
    {
        Task<BaseResultModel> CreateFundraisingRequest(CreateFundraisingModel createFundraisingModel);
        Task<BaseResultModel> CreateFundWithdrawalRequest(CreateFundWithdrawalModel createFundWithdrawalModel);
        Task<BaseResultModel> RemindFundraisingAsync(RemindFundraisingModel remindFundraisingModel);
        Task<BaseResultModel> GetPendingRequestsAsync(Guid groupId, PaginationParameter paginationParameters);
        Task<BaseResultModel> GetPendingRequestDetailAsync(Guid requestId);

        // group transaction
        Task<BaseResultModel> GetTransactionByGroupIdAsync(Guid groupId, PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> CreateGroupTransactionAsync(CreateGroupTransactionModel model, string currentEmail);
        Task<BaseResultModel> GetGroupTransactionDetailsAsync(Guid transactionId);
        Task<BaseResultModel> UpdateGroupTransactionAsync(UpdateGroupTransactionModel model);
        Task<BaseResultModel> DeleteGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> ResponseGroupTransactionAsync(ResponseGroupTransactionModel model);
        Task<BaseResultModel> RejectGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> CreateGroupTransactionVoteAsync(CreateGroupTransactionVoteModel model);
        Task<BaseResultModel> UpdateGroupTransactionVoteAsync(UpdateGroupTransactionVoteModel model);
        Task<BaseResultModel> DeleteGroupTransactionVoteAsync(Guid voteId);
    }
}
