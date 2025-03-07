using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using System;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IUserSpendingModelService
    {
        Task<BaseResultModel> ChooseSpendingModelAsync(ChooseSpendingModelModel model);
        Task<BaseResultModel> SwitchSpendingModelAsync(SwitchSpendingModelModel model);
        Task<BaseResultModel> CancelSpendingModelAsync(Guid spendingModelId);
        Task<BaseResultModel> GetCurrentSpendingModelAsync();
        Task<BaseResultModel> GetUsedSpendingModelByIdAsync(Guid id);
        Task<BaseResultModel> GetUsedSpendingModelsPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetChartCurrentSpendingModelAsync();
        Task<BaseResultModel> GetTransactionsByUserSpendingModelAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter, Guid userSpendingModelId);
        Task<BaseResultModel> UpdateExpiredSpendingModelsAsync();
    }
}
