using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.NotificationModels
{
    public class NotificationModel : BaseEntity
    {
        public bool? IsRead { get; set; }

        public string? Title { get; set; }

        public string? TitleUnsign { get; set; }

        public string? Message { get; set; }

        public Guid? EntityId { get; set; }

        public Guid? UserId { get; set; }

        public string? Type { get; set; }
    }
}
