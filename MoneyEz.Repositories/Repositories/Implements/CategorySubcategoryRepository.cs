using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class CategorySubcategoryRepository : GenericRepository<CategorySubcategory>, ICategorySubcategoryRepository
    {
        private readonly MoneyEzContext _context;

        public CategorySubcategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Subcategory>> GetSubcategoriesBySpendingModelId(Guid spendingModelId)
        {
            var categories = await _context.SpendingModelCategories
                .Where(smc => smc.SpendingModelId == spendingModelId)
                .Select(smc => smc.CategoryId)
                .ToListAsync();

            return await _context.CategorySubcategory
                .Where(cs => categories.Contains(cs.CategoryId))
                .Select(cs => cs.Subcategory)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryBySubcategoryId(Guid subcategoryId)
        {
            return await _context.CategorySubcategory
                .Where(cs => cs.SubcategoryId == subcategoryId)
                .Select(cs => cs.Category)
                .FirstOrDefaultAsync();
        }
    }

}
