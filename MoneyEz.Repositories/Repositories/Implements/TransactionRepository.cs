using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly MoneyEzContext _context;

        public TransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByGroupIdAsync(Guid groupId)
        {
            return await _context.Transactions
                .Where(t => t.GroupId == groupId && !t.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdIncludeAsync(Guid id)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Group)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(
            Expression<Func<Transaction, bool>>? filter = null,
            Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>? orderBy = null)
        {
            IQueryable<Transaction> query = _context.Transactions;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Pagination<Transaction>> GetPaginatedTransactionsAsync(int pageIndex, int pageSize)
        {
            var query = _context.Transactions.Where(t => !t.IsDeleted);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new Pagination<Transaction>(items, totalItems, pageIndex, pageSize);
        }

    }
}
