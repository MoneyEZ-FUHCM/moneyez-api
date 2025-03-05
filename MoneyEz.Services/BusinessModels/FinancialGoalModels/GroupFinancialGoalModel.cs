using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GroupFinancialGoalModel : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public required string Name { get; set; }
        public required string NameUnsign { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public FinancialGoalStatus Status { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
    }
}