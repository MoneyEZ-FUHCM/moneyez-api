using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.UserModels;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GroupMemberModel : BaseEntity
    {
        public Guid GroupId { get; set; }

        public Guid UserId { get; set; }

        public decimal? ContributionPercentage { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }

        public decimal TotalContribution { get; set; } = 0;

        public int TransactionCount { get; set; } = 0;

        public UserModel? UserInfo { get; set; }
    }
}