using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class TransactionFilter : FilterBase
    {
        [FromQuery(Name = "group_id")]
        public Guid? GroupId { get; set; }

        [FromQuery(Name = "user_id")]
        public Guid? UserId { get; set; }
        //public Guid? SubcategoryId { get; set; }
        //public string? Type { get; set; }
        //public string? Status { get; set; }
        //public DateTime? FromDate { get; set; }
        //public DateTime? ToDate { get; set; }
    }
}
