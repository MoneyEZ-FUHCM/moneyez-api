using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class AddGroupFinancialGoalModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public required string Name { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền mục tiêu phải lớn hơn 0.")]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public FinancialGoalStatus Status = FinancialGoalStatus.PENDING;
        [Required]
        public DateTime Deadline { get; set; }

        public ApprovalStatus ApprovalStatus = ApprovalStatus.PENDING;
    }

}
