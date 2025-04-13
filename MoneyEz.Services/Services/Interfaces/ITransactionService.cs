using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.BusinessModels.WebhookModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ITransactionService
    {
        #region personal
        Task<BaseResultModel> GetAllTransactionsForUserAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId);
        Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model, string email);
        Task<BaseResultModel> UpdateTransactionAsync(UpdateTransactionModel model);
        Task<BaseResultModel> DeleteTransactionAsync(Guid transactionId);
        #endregion personal
      
        Task<BaseResultModel> GetAllTransactionsForAdminAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter);

        #region group
        Task<BaseResultModel> GetTransactionByGroupIdAsync(Guid groupId, 
            PaginationParameter paginationParameter, TransactionFilter transactionFilter);
        Task<BaseResultModel> CategorizeTransactionAsync(CategorizeTransactionModel model);
        Task<BaseResultModel> CreateGroupTransactionAsync(CreateGroupTransactionModel model, string currentEmail);
        Task<BaseResultModel> GetGroupTransactionDetailsAsync(Guid transactionId);
        Task<BaseResultModel> UpdateGroupTransactionAsync(UpdateGroupTransactionModel model);
        Task<BaseResultModel> DeleteGroupTransactionAsync(Guid transactionId);

        Task<BaseResultModel> ResponseGroupTransactionAsync(ResponseGroupTransactionModel model);
        Task<BaseResultModel> RejectGroupTransactionAsync(Guid transactionId);
        Task<BaseResultModel> CreateGroupTransactionVoteAsync(CreateGroupTransactionVoteModel model);
        Task<BaseResultModel> UpdateGroupTransactionVoteAsync(UpdateGroupTransactionVoteModel model);
        Task<BaseResultModel> DeleteGroupTransactionVoteAsync(Guid voteId);

        #endregion group

        #region python webhook
        Task<BaseResultModel> UpdateTransactionWebhook(WebhookPayload webhookPayload);
        Task<BaseResultModel> CreateTransactionPythonService(CreateTransactionPythonModel createTransactionPythonModel);
        Task<BaseResultModel> CreateTransactionPythonServiceV2(CreateTransactionPythonModelV2 createTransactionPythonModel);
        Task<BaseResultModel> GetTransactionHistorySendToPythons(Guid userId, TransactionFilter transactionFilter);

        #endregion python webhook

        #region report
        Task<BaseResultModel> GetYearReportAsync(int year, ReportTransactionType type);
        Task<BaseResultModel> GetCategoryYearReportAsync(int year, ReportTransactionType type);
        Task<BaseResultModel> GetAllTimeReportAsync();
        Task<BaseResultModel> GetAllTimeCategoryReportAsync(ReportTransactionType type);
        Task<BaseResultModel> GetBalanceYearReportAsync(int year);

        #endregion report
    }
}
