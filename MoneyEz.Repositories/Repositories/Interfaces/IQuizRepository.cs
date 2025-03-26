using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Pagination<Quiz>> GetAllAsyncPagingInclude(PaginationParameter paginationParameter);
        Task<Quiz?> GetByIdAsyncInclude(Guid id);
        
    }
}
