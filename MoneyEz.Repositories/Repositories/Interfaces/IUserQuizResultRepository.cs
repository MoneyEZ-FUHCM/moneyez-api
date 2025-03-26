using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IUserQuizResultRepository : IGenericRepository<UserQuizResult>
    {
        Task<Pagination<UserQuizResult>> GetAllUserQuizResultsAsync(PaginationParameter paginationParameter);
        Task<UserQuizResult?> GetUserQuizResultByIdAsync(Guid id);
        Task<Pagination<UserQuizResult>> GetUserQuizResultsByUserIdAsync(Guid userId, PaginationParameter paginationParameter);
    }
}
