using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IFixedTransactionRepository : IGenericRepository<FixedTransaction>
    {
        Task<Pagination<FixedTransaction>> GetPaginatedFixedTransactionsAsync(int pageIndex, int pageSize);// get all fixed transactions with pagination
    }
}
