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
using MoneyEz.Repositories.Commons.Filters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task<Pagination<Quiz>> GetAllQuizzesPaginatedAsync(PaginationParameter paginationParameter, FilterBase filter)
        {
            var quizzes = ApplyQuizFiltering(_context.Quizzes.AsQueryable(), filter);

            var count = await quizzes.CountAsync();
            var items = await quizzes
                .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                .Take(paginationParameter.PageSize)
                .ToListAsync();

            return new Pagination<Quiz>(items, count, paginationParameter.PageIndex, paginationParameter.PageSize);
        }

        private IQueryable<Quiz> ApplyQuizFiltering(IQueryable<Quiz> query, FilterBase filter)
        {
            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(q => q.IsDeleted == filter.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchTerm = filter.Search.Trim();

                // If field is specified, search by that field only
                if (!string.IsNullOrWhiteSpace(filter.Field))
                {
                    switch (filter.Field.ToLower())
                    {
                        case "title":
                            query = query.Where(u => u.Title.Contains(searchTerm));
                            break;
                        case "description":
                            query = query.Where(u => u.Description.Contains(searchTerm));
                            break;
                    }
                }
                else
                {
                    // If no field specified, search across all searchable fields
                    query = query.Where(u =>
                        u.Title.Contains(searchTerm) ||
                        u.Description.Contains(searchTerm)
                    );
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.Equals("asc", StringComparison.OrdinalIgnoreCase);

                query = filter.SortBy.ToLower() switch
                {
                    "title" => isAscending ? query.OrderBy(q => q.Title) : query.OrderByDescending(q => q.Title),
                    "createddate" => isAscending ? query.OrderBy(q => q.CreatedDate) : query.OrderByDescending(q => q.CreatedDate),
                    _ => query.OrderByDescending(q => q.CreatedDate) // Default sort by created date desc
                };
            }
            else
            {
                // Default sorting by created date descending if no sort specified
                query = query.OrderByDescending(q => q.CreatedDate);
            }

            return query;
        }
    }
}
