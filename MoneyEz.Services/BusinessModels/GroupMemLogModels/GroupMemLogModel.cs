using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupMemLogModels
{
    public class GroupMemLogModel : BaseEntity
    {
        public Guid GroupMemberId { get; set; }

        public string? ChangeType { get; set; }

        public string? ChangeDiscription { get; set; }
    }
}
