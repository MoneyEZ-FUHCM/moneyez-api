using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class SetRoleGroupModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public RoleGroup RoleGroup { get; set; }
    }
}
