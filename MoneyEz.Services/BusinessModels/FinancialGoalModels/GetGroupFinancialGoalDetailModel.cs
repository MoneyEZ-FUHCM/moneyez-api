using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GetGroupFinancialGoalDetailModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid GoalId { get; set; }
    }
}
