using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GetGroupFinancialGoalsModel
    {
        [Required]
        public Guid GroupId { get; set; }
    }
}
