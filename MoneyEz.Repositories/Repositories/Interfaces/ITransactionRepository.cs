
using MoneyEz.Repositories.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetTransactionsByGroupIdAsync(Guid groupId);
    }
}