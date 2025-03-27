using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFundLogModels
{
    public class GroupFundLogModel : BaseEntity
    {
        public Guid GroupId { get; set; }

        public string? ChangedBy { get; set; }

        public string? ChangeDescription { get; set; }

        public string? Action { get; set; }

        public string? ImageUrl { get; set; }
    }
}
