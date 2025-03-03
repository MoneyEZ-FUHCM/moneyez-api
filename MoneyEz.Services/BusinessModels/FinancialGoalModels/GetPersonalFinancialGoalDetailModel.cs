using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GetPersonalFinancialGoalDetailModel
    {
        [Required]
        public Guid GoalId { get; set; }
    }
}
