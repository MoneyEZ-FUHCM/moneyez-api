using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly MoneyEzContext _context;

        public QuizRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Quiz>> GetAllAsyncPagingInclude(PaginationParameter paginationParameter)
        {
            var quizzes = await ToPaginationIncludeAsync(
                paginationParameter,
                include: q => q.Include(x => x.Questions)
                    .ThenInclude(q => q.AnswerOptions));

            return quizzes ?? new Pagination<Quiz>();
        }

        public async Task<Quiz?> GetByIdAsyncInclude(Guid id)
        {
            return await _context.Set<Quiz>()
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
        }

        public async Task<Quiz?> GetActiveQuizAsync()
        {
            return await _context.Set<Quiz>()
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Status == CommonsStatus.ACTIVE && !q.IsDeleted);
        }

        public async Task DeactivateAllQuizzesAsync()
        {
            var activeQuizzes = await _context.Set<Quiz>()
                .Where(q => q.Status == CommonsStatus.ACTIVE && !q.IsDeleted)
                .ToListAsync();

            foreach (var quiz in activeQuizzes)
            {
                quiz.Status = CommonsStatus.INACTIVE;
                _context.Set<Quiz>().Update(quiz);
            }

            await _context.SaveChangesAsync();
        }
    }
}
