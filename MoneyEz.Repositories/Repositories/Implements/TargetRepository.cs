using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class TargetRepository : GenericRepository<Target>, ITargetRepository
    {
        private readonly MoneyEzContext _context;

        public TargetRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Target>> GetPaginatedTargetsAsync(int pageIndex, int pageSize)
        {
            var targets = _context.Targets.AsNoTracking();
            var totalItems = await targets.CountAsync();
            var paginatedTargets = await targets.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new Pagination<Target>(paginatedTargets, totalItems, pageIndex, pageSize);
        }

        public async Task<Target?> GetTargetByIdAsync(Guid id)
        {
            return await _context.Targets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }
    }
}
