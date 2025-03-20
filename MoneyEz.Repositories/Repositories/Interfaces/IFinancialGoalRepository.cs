using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IFinancialGoalRepository : IGenericRepository<FinancialGoal>
    {
        Task<FinancialGoal?> GetActiveGoalByUserAndSubcategory(Guid userId, Guid subcategoryId);
    }
}
