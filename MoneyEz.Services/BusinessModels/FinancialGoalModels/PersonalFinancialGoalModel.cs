using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class PersonalFinancialGoalModel : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid SubcategoryId { get; set; }
        public string? SubcategoryName { get; set; }
        public string? SubcategoryIcon { get; set; }
        public string? Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public string? Status { get; set; }
        public bool IsSaving { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public GoalPredictionModel Prediction { get; set; }
    }
}