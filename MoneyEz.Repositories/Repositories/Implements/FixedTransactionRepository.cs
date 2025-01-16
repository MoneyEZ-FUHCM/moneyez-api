using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class FixedTransactionRepository : GenericRepository<FixedTransaction>, IFixedTransactionRepository
    {
        private readonly MoneyEzContext _context;

        public FixedTransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<FixedTransaction>> GetPaginatedFixedTransactionsAsync(int pageIndex, int pageSize)
        {
            var query = _context.FixedTransactions.Where(ft => !ft.IsDeleted);
            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(ft => ft.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new Pagination<FixedTransaction>(items, totalItems, pageIndex, pageSize);
        }
    }
}
