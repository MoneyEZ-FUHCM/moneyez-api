using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
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
    }
}
