using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class UpdateGroupModel : CreateGroupModel
    {
        public Guid Id { get; set; }
    }
}
