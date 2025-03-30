using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ISpendingModelRepository : IGenericRepository<SpendingModel>
    {
        Task<Pagination<SpendingModel>> GetSpendingModelsFilterAsync(
            PaginationParameter paginationParameter,
            SpendingModelFilter spendingModelFilter,
            Expression<Func<SpendingModel, bool>>? condition = null,
            Func<IQueryable<SpendingModel>, IIncludableQueryable<SpendingModel, object>>? include = null);
    }
}