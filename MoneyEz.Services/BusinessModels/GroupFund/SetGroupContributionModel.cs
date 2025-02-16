using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class SetGroupContributionModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public List<MemberContributionModel> MemberContributions { get; set; } = new List<MemberContributionModel>();
    }

    public class MemberContributionModel
    {
        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public decimal Contribution { get; set; } = 0;
    }
}
