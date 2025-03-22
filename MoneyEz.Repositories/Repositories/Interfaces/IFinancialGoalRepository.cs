using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using System.Linq.Expressions;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IFinancialGoalRepository : IGenericRepository<FinancialGoal>
    {
        Task<FinancialGoal?> GetActiveGoalByUserAndSubcategory(Guid userId, Guid subcategoryId);

        Task<Pagination<FinancialGoal>> GetPersonalFinancialGoalsFilterAsync(
            Guid userId, PaginationParameter paginationParameter, FinancialGoalFilter financialGoalFilter,
            Expression<Func<FinancialGoal, bool>> condition = null,
            Func<IQueryable<FinancialGoal>, IIncludableQueryable<FinancialGoal, object>>? include = null
        );
    }
}
