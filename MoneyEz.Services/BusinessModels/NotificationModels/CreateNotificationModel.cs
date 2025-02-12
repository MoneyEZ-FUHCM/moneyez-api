using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.NotificationModels
{
    public class CreateNotificationModel
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public List<Guid>? UserIds { get; set; } = new List<Guid>();
    }
}
