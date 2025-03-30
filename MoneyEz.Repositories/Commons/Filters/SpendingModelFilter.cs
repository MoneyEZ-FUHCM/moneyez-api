using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class SpendingModelFilter : FilterBase
    {
        [FromQuery(Name = "name")]
        public string? Name { get; set; }

        [FromQuery(Name = "is_template")]
        public bool? IsTemplate { get; set; }

        [FromQuery(Name = "category_id")]
        public Guid? CategoryId { get; set; }
    }
}
