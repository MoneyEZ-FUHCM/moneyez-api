using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class RemindFundraisingModel
    {
        [Required(ErrorMessage = "Group Id is required")]
        public Guid GroupId { get; set; }
        public List<MemberRemindFundraisingModel> Members { get; set; } = new List<MemberRemindFundraisingModel>();
    }

    public class MemberRemindFundraisingModel
    {
        [Required(ErrorMessage = "Member Id is required")]
        public Guid MemberId { get; set; }

        [Required(ErrorMessage = "Member amount is required")]
        public decimal Amount { get; set; }
    }
}
