using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class TransactionFilter : FilterBase
    {
        [FromQuery(Name = "category_id")]
        public Guid? SubcategoryId { get; set; }

        [FromQuery(Name = "type")]
        public TransactionType? Type { get; set; }

        [FromQuery(Name = "from_date")]
        public DateTime? FromDate { get; set; }

        [FromQuery(Name = "to_date")]
        public DateTime? ToDate { get; set; }

        [FromQuery(Name = "status")]
        public string? Status { get; set; }
    }
}
