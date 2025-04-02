using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserQuizResultRepository : GenericRepository<UserQuizResult>, IUserQuizResultRepository
    {
        private readonly MoneyEzContext _dbContext;

        public UserQuizResultRepository(MoneyEzContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<UserQuizResult> GetUserQuizResultByIdAsync(Guid id)
        {
            return await _dbContext.UserQuizResults
                .Include(uqr => uqr.Quiz)
                .FirstOrDefaultAsync(uqr => uqr.Id == id);
        }

        public async Task<List<UserQuizResult>> GetUserQuizResultsByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserQuizResults
                .Include(uqr => uqr.Quiz)
                .Where(uqr => uqr.UserId == userId)
                .OrderByDescending(uqr => uqr.TakenAt)
                .ToListAsync();
        }

        public async Task<Pagination<UserQuizResult>> GetUserQuizResultsByUserIdPaginatedAsync(Guid userId, PaginationParameter paginationParameter)
        {
            var query = _dbContext.UserQuizResults
                .Include(uqr => uqr.Quiz)
                .Where(uqr => uqr.UserId == userId)
                .OrderByDescending(uqr => uqr.TakenAt)
                .AsQueryable();

            var count = await query.CountAsync();
            var items = await query
                .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                .Take(paginationParameter.PageSize)
                .ToListAsync();

            return new Pagination<UserQuizResult>(items, count, paginationParameter.PageIndex, paginationParameter.PageSize);
        }

        public async Task<UserQuizResult> CreateUserQuizResultAsync(UserQuizResult userQuizResult)
        {
            var quiz = await _dbContext.Quizzes.FindAsync(userQuizResult.QuizId);
            if (quiz != null)
            {
                userQuizResult.QuizVersion = quiz.Version;
            }
            
            userQuizResult.TakenAt = DateTime.Now;
            
            await _dbContext.UserQuizResults.AddAsync(userQuizResult);
            await _dbContext.SaveChangesAsync();
            
            return userQuizResult;
        }
    }
}
