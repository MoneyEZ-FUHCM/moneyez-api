using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly MoneyEzContext _context;

        public PostRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<Post>> GetPostsByFilter(PaginationParameter paginationParameter, PostFilter filter)
        {
            var query = _context.Posts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                string search = filter.Search.ToLower().Trim();
                query = query.Where(p => p.Title.ToLower().Contains(search) || 
                                      (p.ShortContent != null && p.ShortContent.ToLower().Contains(search)));
            }

            // Apply IsDeleted filter
            query = query.Where(p => p.IsDeleted == filter.IsDeleted);

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var sortField = filter.SortBy.ToLower();
                var isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";

                query = sortField switch
                {
                    "title" => isAscending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                    "createdat" => isAscending ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                    "updatedat" => isAscending ? query.OrderBy(p => p.UpdatedDate) : query.OrderByDescending(p => p.UpdatedDate),
                    _ => isAscending ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate)
                };
            }
            else
            {
                // Default sort by creation date descending
                query = query.OrderByDescending(p => p.CreatedDate);
            }

            return await ToPaginationQueryable(query, paginationParameter);
        }

        private async Task<Pagination<Post>> ToPaginationQueryable(IQueryable<Post> source, PaginationParameter paginationParameter)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                .Take(paginationParameter.PageSize)
                .ToListAsync();

            return new Pagination<Post>(items, count, paginationParameter.PageIndex, paginationParameter.PageSize);
        }
    }
}
