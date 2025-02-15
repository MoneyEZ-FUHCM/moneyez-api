using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GroupInviteRedisModel
    {
        public string InviteToken { get; set; } = "";

        public Guid UserId { get; set; }
        
        public Guid GroupId { get; set; }
    }
}
