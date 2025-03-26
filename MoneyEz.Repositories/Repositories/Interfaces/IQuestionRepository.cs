using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<List<Question>> GetByQuizIdAsync(Guid quizId);
    }
}
