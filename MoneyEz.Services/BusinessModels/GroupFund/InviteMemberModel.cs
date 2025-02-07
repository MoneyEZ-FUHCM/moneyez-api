using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class InviteMemberModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public string Email { get; set; } = "";
    }
}
