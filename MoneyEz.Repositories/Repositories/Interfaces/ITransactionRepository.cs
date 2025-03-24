using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<decimal> GetTotalIncomeAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalExpenseAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate);

        Task<decimal> GetTotalExpenseByCategory(Guid userId, Guid categoryId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalIncomeByCategory(Guid userId, Guid categoryId, DateTime startDate, DateTime endDate);

        Task<Pagination<Transaction>> GetTransactionsFilterAsync(PaginationParameter paginationParameter,
                        TransactionFilter transactionFilter,
                        Func<IQueryable<Transaction>, IIncludableQueryable<Transaction, object>>? include = null);

        Task<decimal> GetToalIncomeByUserSpendingModelAsync(Guid userSpendingModelId);

    }
}
