using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GroupMemberModel : BaseEntity
    {
        public Guid GroupId { get; set; }

        public Guid UserId { get; set; }

        public decimal? ContributionPercentage { get; set; }

        public RoleGroup? Role { get; set; }

        public GroupMemberStatus? Status { get; set; }


    }
}