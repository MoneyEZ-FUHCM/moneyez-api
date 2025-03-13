using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        public Task<List<Notification>> GetAllNotificationsByUserIdAsync(Guid userId);

        public Task<Pagination<Notification>> GetNotificationsFilter(PaginationParameter paginationParameter, NotificationFilter filter,
            Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>? include = null);
    }
}
