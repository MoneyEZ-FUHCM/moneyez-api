using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SubCategoryRepository : GenericRepository<Subcategory>, ISubCategoryRepository
    {
        private readonly MoneyEzContext _context;

        public SubCategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Subcategory>> GetAllAsync()
        {
            return await _context.Subcategories.Where(sc => !sc.IsDeleted).ToListAsync();
        }

        public async Task<Subcategory?> GetByIdAsync(Guid id)
        {
            return await _context.Subcategories.FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
        }
    }
}
