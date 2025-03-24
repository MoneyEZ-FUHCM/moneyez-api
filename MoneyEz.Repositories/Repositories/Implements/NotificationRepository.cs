using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly MoneyEzContext _context;

        public NotificationRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(Guid userId)
        {
            return await _context.Notifications.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<Pagination<Notification>> GetNotificationsFilter(PaginationParameter paginationParameter, NotificationFilter filter, 
            Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>? include = null)
        {
            var query = _context.Notifications.AsQueryable();

            // apply filter
            query = ApplyNotificationFiltering(query, filter);

            if (include != null)
            {
                query = include(query);
            }

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Notification>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<Notification> ApplyNotificationFiltering(IQueryable<Notification> query, NotificationFilter filter)
        {

            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(u => u.IsDeleted == filter.IsDeleted);

            if (filter.UserId.HasValue)
            {
                query = query.Where(t => t.UserId == filter.UserId.Value);
            }

            if (filter.IsRead.HasValue)
            {
                query = query.Where(t => t.IsRead == filter.IsRead.Value);
            }

            if (!string.IsNullOrEmpty(filter.Type))
            {
                if (filter.Type == "group")
                {
                    query = query.Where(t => t.Type == NotificationType.GROUP || t.Type == NotificationType.GROUP_INVITE);
                }
                else if (filter.Type == "user")
                {
                    query = query.Where(t => t.Type == NotificationType.USER);
                }
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
                        case "message":
                            query = query.Where(u => u.Message.Contains(searchTerm));
                            break;
                        case "title":
                            query = query.Where(u => u.TitleUnsign.Contains(searchTerm) || u.Title.Contains(searchTerm));
                            break;
                    }
                }
                else
                {
                    // If no field specified, search across all searchable fields
                    query = query.Where(u =>
                        u.Title.Contains(searchTerm) ||
                        u.TitleUnsign.Contains(searchTerm) ||
                        u.Message.Contains(searchTerm)
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
                        query = isDescending ? query.OrderByDescending(u => u.Title) : query.OrderBy(u => u.Title);
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
