using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<Quiz> GetActiveQuizAsync()
        {
            return await _context.Quizzes
                .Where(q => q.Status == Enums.CommonsStatus.ACTIVE)
                .FirstOrDefaultAsync();
        }
        
        public async Task<Quiz> CreateQuizVersionAsync(Quiz quiz)
        {
            quiz.Version = DateTime.Now.ToString("yyyyMMddHHmm");
            
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();
            
            return quiz;
        }
        
        public async Task<Quiz> UpdateQuizAsync(Quiz quiz)
        {
            // Preserve original ID but create a new version
            var existingQuiz = await _context.Quizzes.FindAsync(quiz.Id);
            if (existingQuiz == null)
                return null;
                
            // Update the version
            quiz.Version = DateTime.Now.ToString("yyyyMMddHHmm");
            
            // Update the values
            _context.Entry(existingQuiz).CurrentValues.SetValues(quiz);
            await _context.SaveChangesAsync();
            
            return existingQuiz;
        }

        public async Task<Pagination<Quiz>> GetAllQuizzesPaginatedAsync(PaginationParameter paginationParameter)
        {
            var quizzes = _context.Quizzes
                .OrderByDescending(q => q.CreatedDate)
                .AsQueryable();

            var count = await quizzes.CountAsync();
            var items = await quizzes
                .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                .Take(paginationParameter.PageSize)
                .ToListAsync();

            return new Pagination<Quiz>(items, count, paginationParameter.PageIndex, paginationParameter.PageSize);
        }
    }
}
