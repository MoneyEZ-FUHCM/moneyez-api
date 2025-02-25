using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class FilterBase
    {
        public string? Search { get; set; }

        public string? Fields { get; set; }

        public string? SortBy { get; set; }

        public string? Dir { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
