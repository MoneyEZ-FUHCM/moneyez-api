using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Linq.Expressions;
using System.Linq;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SpendingModelRepository : GenericRepository<SpendingModel>, ISpendingModelRepository
    {
        private readonly MoneyEzContext _context;

        public SpendingModelRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<SpendingModel>> GetSpendingModelsFilterAsync(
            PaginationParameter paginationParameter,
            SpendingModelFilter spendingModelFilter,
            Expression<Func<SpendingModel, bool>>? condition = null,
            Func<IQueryable<SpendingModel>, IIncludableQueryable<SpendingModel, object>>? include = null)
        {
            var query = _context.SpendingModels.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (condition != null)
            {
                query = query.Where(condition);
            }

            // apply filter
            query = ApplySpendingModelFiltering(query, spendingModelFilter);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                 .Take(paginationParameter.PageSize)
                                 .AsNoTracking()
                                 .ToListAsync();

            var result = new Pagination<SpendingModel>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<SpendingModel> ApplySpendingModelFiltering(IQueryable<SpendingModel> query, SpendingModelFilter filter)
        {
            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(s => s.IsDeleted == filter.IsDeleted);

            // Filter by name if provided
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(s => s.Name.Contains(filter.Name) || s.NameUnsign.Contains(filter.Name));
            }

            // Filter by IsTemplate if provided
            if (filter.IsTemplate.HasValue)
            {
                query = query.Where(s => s.IsTemplate == filter.IsTemplate.Value);
            }

            // Filter by CategoryId if provided
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(s => s.SpendingModelCategories.Any(smc => smc.CategoryId == filter.CategoryId.Value));
            }

            // Apply search across multiple fields if provided
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(s =>
                    s.Name.Contains(filter.Search) ||
                    s.NameUnsign.Contains(filter.Search) ||
                    s.Description.Contains(filter.Search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";

                query = filter.SortBy.ToLower() switch
                {
                    "name" => isAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                    "created_date" => isAscending ? query.OrderBy(s => s.CreatedDate) : query.OrderByDescending(s => s.CreatedDate),
                    _ => query.OrderBy(s => s.Name) // Default sort by creation date desc
                };
            }
            else
            {
                // Default sorting by creation date descending if no sort specified
                query = query.OrderBy(s => s.Name);
            }

            return query;
        }
    }
}
