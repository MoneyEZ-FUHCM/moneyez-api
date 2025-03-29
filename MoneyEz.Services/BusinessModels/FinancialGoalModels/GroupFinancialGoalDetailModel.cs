using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GroupFinancialGoalDetailModel : GroupFinancialGoalModel
    {
        public List<GroupMemberContributionModel> MemberContributions { get; set; } 
            = new List<GroupMemberContributionModel>();
        public decimal TotalCurrentAmount { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}
