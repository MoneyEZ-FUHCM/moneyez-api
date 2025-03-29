using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class RecurringTransactionFilter : FilterBase
    {
        public Guid? SubcategoryId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
