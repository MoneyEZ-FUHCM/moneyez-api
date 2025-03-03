using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class AddPersonalFinancialGoalModel
    {
        [Required]
        public Guid SubcategoryId { get; set; }

        [Required]
        public required string Name { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0.")]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount = 0;

        [Required]
        public DateTime Deadline { get; set; }
    }
}
