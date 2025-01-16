using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using System.Linq.Expressions;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId); // Get transactions by user id
        Task<IEnumerable<Transaction>> GetTransactionsByGroupIdAsync(Guid groupId); // Get transactions by group id
        Task<Transaction?> GetTransactionByIdIncludeAsync(Guid id); // Get transaction by id
        Task<IEnumerable<Transaction>> GetTransactionsAsync(
            Expression<Func<Transaction, bool>>? filter = null,
            Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>? orderBy = null); // Get transactions
        Task<Pagination<Transaction>> GetPaginatedTransactionsAsync(int pageIndex, int pageSize); //get paginated transactions
    }
}
