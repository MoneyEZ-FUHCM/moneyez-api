using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons.Filters
{
    public class GroupFilter : FilterBase
    {
        public Guid? UserId { get; set; }
        public GroupStatus? Status { get; set; }
    }
}
