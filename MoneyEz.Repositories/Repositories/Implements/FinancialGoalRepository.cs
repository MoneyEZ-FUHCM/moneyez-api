using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class FinancialGoalRepository : GenericRepository<FinancialGoal>, IFinancialGoalRepository
    {
        private readonly MoneyEzContext _context;

        public FinancialGoalRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FinancialGoal?> GetActiveGoalByUserAndSubcategory(Guid userId, Guid subcategoryId)
        {
            return await _context.FinancialGoals
                .Where(fg => fg.UserId == userId
                          && fg.SubcategoryId == subcategoryId
                          && fg.Status == FinancialGoalStatus.ACTIVE
                          && !fg.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}
