using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupMember
{
    public class CreateGroupMemberModel
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public decimal? ContributionPercentage { get; set; }
        public int? Role { get; set; }
        public int? Status { get; set; }
    }

}