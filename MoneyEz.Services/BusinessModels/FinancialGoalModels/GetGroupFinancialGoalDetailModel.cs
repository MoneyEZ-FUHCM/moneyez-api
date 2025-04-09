using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GetGroupFinancialGoalDetailModel
    {
        [Required]
        [FromQuery(Name = "groupId")]
        public Guid GroupId { get; set; }

        [Required]
        [FromQuery(Name = "goalId")]
        public Guid GoalId { get; set; }
    }
}
