using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        private readonly MoneyEzContext _context;

        public QuestionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Question>> GetByQuizIdAsync(Guid quizId)
        {
            return await _context.Set<Question>()
                                 .Include(q => q.Quiz)
                                 .Where(q => q.QuizId == quizId)
                                 .ToListAsync();
        }
    }
}
