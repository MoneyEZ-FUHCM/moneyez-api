using MoneyEz.Repositories.Entities;
using MoneyEz.Services.Utils;
using System;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class PersonalFinancialGoalModel : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid SubcategoryId { get; set; }
        public string Name { get; set; }
        public string NameUnsign { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public DateTime Deadline { get; set; }
    }
}
