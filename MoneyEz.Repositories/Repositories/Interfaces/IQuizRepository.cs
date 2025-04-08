using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Quiz> GetActiveQuizAsync();
        Task<Quiz> CreateQuizVersionAsync(Quiz quiz);
        Task<Quiz> UpdateQuizAsync(Quiz quiz);
        Task<Pagination<Quiz>> GetAllQuizzesPaginatedAsync(PaginationParameter paginationParameter);
    }
}
