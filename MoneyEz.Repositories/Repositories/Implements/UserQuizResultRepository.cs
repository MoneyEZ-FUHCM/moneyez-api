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
        private readonly MoneyEzContext _context;

        public UserQuizResultRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Pagination<UserQuizResult>> GetAllUserQuizResultsAsync(PaginationParameter paginationParameter)
        {
            var query = _context.Set<UserQuizResult>()
                .Include(uqr => uqr.Quiz)
                .Include(uqr => uqr.User)
                .Include(uqr => uqr.UserQuizAnswers)
                .AsQueryable();

            query = query.OrderBy(uqr => uqr.TakenAt);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                   .Take(paginationParameter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            var result = new Pagination<UserQuizResult>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }
        public async Task<UserQuizResult?> GetUserQuizResultByIdAsync(Guid id)
        {
            var query = _context.Set<UserQuizResult>()
                .Include(uqr => uqr.Quiz)
                .Include(uqr => uqr.User)
                .Include(uqr => uqr.UserQuizAnswers)
                .AsQueryable();

            var userQuizResult = await query.AsNoTracking().FirstOrDefaultAsync(uqr => uqr.Id == id);
            return userQuizResult;
        }
        public async Task<Pagination<UserQuizResult>> GetUserQuizResultsByUserIdAsync(Guid userId, PaginationParameter paginationParameter)
        {
            var query = _context.Set<UserQuizResult>()
                .Include(uqr => uqr.Quiz)
                .Include(uqr => uqr.User)
                .Include(uqr => uqr.UserQuizAnswers)
                .Where(uqr => uqr.UserId == userId)
                .AsQueryable();

            query = query.OrderBy(uqr => uqr.TakenAt);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                   .Take(paginationParameter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            var result = new Pagination<UserQuizResult>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }
    }
}
