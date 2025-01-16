using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SubCategoryRepository : GenericRepository<Subcategory>, ISubCategoryRepository
    {
        private readonly MoneyEzContext _context;

        public SubCategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Subcategory>> GetPaginatedSubcategoriesAsync(int pageIndex, int pageSize)
        {
            var query = _context.Subcategories.Where(sc => !sc.IsDeleted);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new Pagination<Subcategory>(items, totalItems, pageIndex, pageSize);
        }


        public async Task<Subcategory?> GetByIdAsync(Guid id)
        {
            return await _context.Subcategories.FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
        }
    }
}
