using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class FinancialGoalRepository : GenericRepository<FinancialGoal>, IFinancialGoalRepository
    {
        private readonly MoneyEzContext _context;

        public FinancialGoalRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FinancialGoal?> GetActiveGoalByUserAndSubcategory(Guid userId, Guid subcategoryId)
        {
            return await _context.FinancialGoals
                .Where(fg => fg.UserId == userId
                          && fg.SubcategoryId == subcategoryId
                          && fg.Status == FinancialGoalStatus.ACTIVE
                          && !fg.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Pagination<FinancialGoal>> GetPersonalFinancialGoalsFilterAsync(Guid userId, 
            PaginationParameter paginationParameter, FinancialGoalFilter financialGoalFilter,
            Expression<Func<FinancialGoal, bool>>? condition = null,
            Func<IQueryable<FinancialGoal>, IIncludableQueryable<FinancialGoal, object>>? include = null)
        {
            var query = _context.FinancialGoals.Where(u => u.UserId == userId && u.GroupId == null).AsQueryable();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            if (include != null)
            {
                query = include(query);
            }

            // apply filter
            query = ApplyFinancialGoalFiltering(financialGoalFilter, query);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<FinancialGoal>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<FinancialGoal> ApplyFinancialGoalFiltering(FinancialGoalFilter filter, IQueryable<FinancialGoal> query)
        {
            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(u => u.IsDeleted == filter.IsDeleted);

            if (!string.IsNullOrEmpty(filter.SubcategoryCode))
            {
                query = query.Where(t => t.Subcategory.Code == filter.SubcategoryCode);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (filter.Status.ToLower() == "active")
                {
                    query = query.Where(t => t.Status == FinancialGoalStatus.ACTIVE);
                }
                else if (filter.Status.ToLower() == "completed")
                {
                    query = query.Where(t => t.Status == FinancialGoalStatus.COMPLETED);
                }
                else if (filter.Status.ToUpper() == "archived")
                {
                    query = query.Where(t => t.Status == FinancialGoalStatus.ARCHIVED);
                }
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(t => t.StartDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(t => t.Deadline <= filter.ToDate.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchTerm = filter.Search.Trim();

                // If field is specified, search by that field only
                if (!string.IsNullOrWhiteSpace(filter.Field))
                {
                    switch (filter.Field.ToLower())
                    {
                        case "name":
                            query = query.Where(u => u.Name.Contains(searchTerm) || u.NameUnsign.Contains(searchTerm));
                            break;
                    }
                }
                else
                {
                    // If no field specified, search across all searchable fields
                    query = query.Where(u =>
                        u.Name.Contains(searchTerm) ||
                        u.NameUnsign.Contains(searchTerm)
                    );
                }
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";

                query = filter.SortBy.ToLower() switch
                {
                    "name" => isAscending ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
                    "target_amount" => isAscending ? query.OrderBy(t => t.TargetAmount) : query.OrderByDescending(t => t.TargetAmount),
                    "date" => isAscending ? query.OrderBy(t => t.CreatedDate) : query.OrderByDescending(t => t.CreatedDate),
                    _ => query.OrderByDescending(t => t.CreatedDate) // Default sort by date desc
                };
            }
            else
            {
                query = query.OrderByDescending(t => t.CreatedDate);
            }

            return query;
        }
    }
}
