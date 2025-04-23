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
        Task<BaseResultModel> CategorizeTransactionAsync(CategorizeTransactionModel model);

        #region report
        Task<BaseResultModel> GetYearReportAsync(int year, string type);
        Task<BaseResultModel> GetCategoryYearReportAsync(int year, string type);
        Task<BaseResultModel> GetAllTimeReportAsync();
        Task<BaseResultModel> GetBalanceYearReportAsync(int year);
        Task<BaseResultModel> GetAllTimeCategoryReportAsync(string type);

        /*        Task<BaseResultModel> GetCategoryYearReportAsyncV2(int year, string type);*/
        /*        Task<BaseResultModel> GetAllTimeCategoryReportAsyncV2(string type);*/

        #endregion report
    }
}
