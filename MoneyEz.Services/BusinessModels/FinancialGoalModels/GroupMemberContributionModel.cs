using System;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GroupMemberContributionModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        // Actual amounts and percentages
        public decimal CurrentContributionAmount { get; set; }
        // Planned amounts and percentages
        public decimal PlannedContributionPercentage { get; set; }
        public decimal PlannedTargetAmount { get; set; }
        // Progress tracking
        public decimal RemainingAmount { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}
