using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class DeleteFinancialGoalModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
