using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund.GroupInvite
{
    public class InviteMemberModel
    {
        [Required]
        public Guid GroupId { get; set; }

        public List<String> Emails { get; set; } = [];

        public string? Description { get; set; } = "";
    }
}
