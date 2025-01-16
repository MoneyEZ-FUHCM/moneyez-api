using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly MoneyEzContext _context;

        public CategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsCategoryExistsAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted);
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted);
        }

        public async Task<Pagination<Category>> GetPaginatedCategoriesAsync(int pageIndex, int pageSize)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new Pagination<Category>(items, totalItems, pageIndex, pageSize);
        }

    }
}
