using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class CategoryCurrentSpendingModelFiter
    {
        [FromQuery(Name = "type")]
        public string? Type { get; set; }

        [FromQuery(Name = "code")]
        public string? Code { get; set; }

        [FromQuery(Name = "last_used")]
        public bool? IsLastUsed { get; set; }
    }
}
