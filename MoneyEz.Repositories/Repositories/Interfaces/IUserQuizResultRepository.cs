using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IUserQuizResultRepository : IGenericRepository<UserQuizResult>
    {
        Task<UserQuizResult> GetUserQuizResultByIdAsync(Guid id);
        Task<List<UserQuizResult>> GetUserQuizResultsByUserIdAsync(Guid userId);
        Task<Pagination<UserQuizResult>> GetUserQuizResultsByUserIdPaginatedAsync(Guid userId, PaginationParameter paginationParameter);
        Task<UserQuizResult> CreateUserQuizResultAsync(UserQuizResult userQuizResult);
    }
}
