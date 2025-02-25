using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class UpdatePersonalFinancialGoalModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);

        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền mục tiêu phải lớn hơn 0.")]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}
