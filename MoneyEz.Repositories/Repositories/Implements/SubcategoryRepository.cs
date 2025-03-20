using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SubcategoryRepository : GenericRepository<Subcategory>, ISubcategoryRepository
    {
        private readonly MoneyEzContext _context;

        public SubcategoryRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Subcategory>> GetSubcategoriesByFilter(PaginationParameter paginationParameter, SubcategoryFilter filter)
        {
            var query = _context.Subcategories.AsQueryable();

            // apply filter
            query = ApplyCategoriesFiltering(query, filter);

            var itemCount = await query.CountAsync();
            var items = await query.Include(c => c.CategorySubcategories).ThenInclude(cs => cs.Category)
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Subcategory>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<Subcategory> ApplyCategoriesFiltering(IQueryable<Subcategory> query, SubcategoryFilter filter)
        {
            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(u => u.IsDeleted == filter.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchTerm = filter.Search.Trim();

                // If field is specified, search by that field only
                if (!string.IsNullOrWhiteSpace(filter.Field))
                {
                    switch (filter.Field.ToLower())
                    {
                        case "code":
                            query = query.Where(u => u.Code.Contains(searchTerm));
                            break;
                        case "name":
                            query = query.Where(u => u.Name.Contains(searchTerm) || u.NameUnsign.Contains(searchTerm));
                            break;
                    }
                }
                else
                {
                    // If no field specified, search across all searchable fields
                    query = query.Where(u =>
                        u.Code.Contains(searchTerm) ||
                        u.Name.Contains(searchTerm) ||
                        u.NameUnsign.Contains(searchTerm)
                    );
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var isDescending = !string.IsNullOrWhiteSpace(filter.Dir) && filter.Dir.ToLower() == "desc";

                switch (filter.SortBy.ToLower())
                {
                    case "name":
                        query = isDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name);
                        break;
                    case "date":
                        query = isDescending ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate);
                        break;
                }
            }

            return query;
        }
    }
}
