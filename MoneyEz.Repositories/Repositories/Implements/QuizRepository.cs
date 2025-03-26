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
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly MoneyEzContext _context;

        public QuizRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Quiz>> GetAllAsyncPagingInclude(PaginationParameter paginationParameter)
        {
            var query = _context.Set<Quiz>()
                .Include(q => q.Questions)
                    .ThenInclude(question => question.AnswerOptions)
                .AsQueryable();

            query = query.OrderBy(q => q.Title);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                   .Take(paginationParameter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            var result = new Pagination<Quiz>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }
        public async Task<Quiz?> GetByIdAsyncInclude(Guid id)
        {
            var query = _context.Set<Quiz>()
                .Include(q => q.Questions)
                    .ThenInclude(question => question.AnswerOptions)
                .AsQueryable();

            var quiz = await query.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
            return quiz;
        }
        
    }
}
