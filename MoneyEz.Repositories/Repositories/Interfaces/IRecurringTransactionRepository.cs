using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IRecurringTransactionRepository : IGenericRepository<RecurringTransaction>
    {
        Task<Pagination<RecurringTransaction>> GetRecurringTransactionsFilterAsync(
        PaginationParameter paginationParameter,
        RecurringTransactionFilter filter,
        Expression<Func<RecurringTransaction, bool>>? condition = null,
        Func<IQueryable<RecurringTransaction>, IIncludableQueryable<RecurringTransaction, object>>? include = null);
    }
}
