using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly MoneyEzContext _context;

        public UserRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetUserByPhoneAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<List<User>> GetUsersByUserIdsAsync(List<Guid> userIds)
        {
            return await _context.Users.Where(x => userIds.Contains(x.Id)).ToListAsync();
        }

        public async Task<Pagination<User>> GetUsersByFilter(PaginationParameter paginationParameter, UserFilter filter)
        {
            var query = _context.Users.AsQueryable();

            // apply filter
            query = ApplyUserFiltering(query, filter);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<User>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<User> ApplyUserFiltering(IQueryable<User> query, UserFilter filter)
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
                        case "email":
                            query = query.Where(u => u.Email.Contains(searchTerm));
                            break;
                        case "fullname":
                            query = query.Where(u => u.FullName.Contains(searchTerm) || u.NameUnsign.Contains(searchTerm));
                            break;
                        case "phone":
                            query = query.Where(u => u.PhoneNumber.Contains(searchTerm));
                            break;
                        case "address":
                            query = query.Where(u => u.Address.Contains(searchTerm));
                            break;
                    }
                }
                else
                {
                    // If no field specified, search across all searchable fields
                    query = query.Where(u =>
                        u.Email.Contains(searchTerm) ||
                        u.FullName.Contains(searchTerm) ||
                        u.NameUnsign.Contains(searchTerm) ||
                        u.PhoneNumber.Contains(searchTerm) ||
                        (u.Address != null && u.Address.Contains(searchTerm))
                    );
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var isDescending = !string.IsNullOrWhiteSpace(filter.Dir) && filter.Dir.ToLower() == "desc";

                switch (filter.SortBy.ToLower())
                {
                    case "email":
                        query = isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                        break;
                    case "fullname":
                        query = isDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName);
                        break;
                    case "phone":
                        query = isDescending ? query.OrderByDescending(u => u.PhoneNumber) : query.OrderBy(u => u.PhoneNumber);
                        break;
                    case "date":
                        query = isDescending ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate);
                        break;
                    case "status":
                        query = isDescending ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status);
                        break;
                    case "isverified":
                        query = isDescending ? query.OrderByDescending(u => u.IsVerified) : query.OrderBy(u => u.IsVerified);
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
