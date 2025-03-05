using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Implements;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SpendingModelCategoryRepository : GenericRepository<SpendingModelCategory>, ISpendingModelCategoryRepository
    {
        private readonly MoneyEzContext _context;

        public SpendingModelCategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SpendingModelCategory?> GetByModelAndCategory(Guid spendingModelId, Guid categoryId)
        {
            return await _context.SpendingModelCategories
                .FirstOrDefaultAsync(smc => smc.SpendingModelId == spendingModelId && smc.CategoryId == categoryId);
        }
    }
}

