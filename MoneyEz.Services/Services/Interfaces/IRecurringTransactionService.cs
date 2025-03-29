using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.RecurringTransactionModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IRecurringTransactionService
    {
        Task<BaseResultModel> AddRecurringTransactionAsync(CreateRecurringTransactionModel model);
        Task<BaseResultModel> GetAllRecurringTransactionsAsync(PaginationParameter paginationParameter, RecurringTransactionFilter filter);
        Task<BaseResultModel> GetRecurringTransactionByIdAsync(Guid id);
        Task<BaseResultModel> UpdateRecurringTransactionAsync(UpdateRecurringTransactionModel model);
        Task<BaseResultModel> DeleteRecurringTransactionAsync(Guid id);
        Task GenerateTransactionsFromRecurringAsync();
    }
}
