using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupRepository : GenericRepository<GroupFund>, IGroupFundRepository
    {
        private readonly MoneyEzContext _context;

        public GroupRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<GroupFund>> GetByAccountBankId(Guid accountBankId)
        {
            return _context.GroupFunds.Where(x => x.AccountBankId == accountBankId).ToListAsync();
        }

        public async Task<Pagination<GroupFund>> GetGroupFundsFilterAsync(
            PaginationParameter paginationParameter, GroupFilter filter,
            Func<IQueryable<GroupFund>, IIncludableQueryable<GroupFund, object>>? include = null)
        {
            var query = _context.GroupFunds.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(x => x.GroupMembers.Any(gm => gm.UserId == filter.UserId && gm.Status != GroupMemberStatus.INACTIVE));
            }

            // apply filter
            query = ApplyGroupFiltering(query, filter);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<GroupFund>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<GroupFund> ApplyGroupFiltering(IQueryable<GroupFund> query, GroupFilter filter)
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
                        case "name":
                            query = query.Where(u => u.Name.Contains(searchTerm));
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

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status);
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
                    default:
                        // Default sort by Id
                        query = isDescending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id);
                        break;
                }
            }
            else
            {
                // Default sort by Id if no sort specified
                query = query.OrderBy(u => u.Id);
            }

            return query;
        }
    }
}
