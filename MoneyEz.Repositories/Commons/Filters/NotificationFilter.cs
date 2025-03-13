using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class NotificationFilter : FilterBase
    {
        [FromQuery(Name = "type")]
        public string? Type { get; set; }

        [FromQuery(Name = "user_id")]
        public Guid? UserId { get; set; }

        [FromQuery(Name = "is_read")]
        public bool? IsRead { get; set; }
    }
}
