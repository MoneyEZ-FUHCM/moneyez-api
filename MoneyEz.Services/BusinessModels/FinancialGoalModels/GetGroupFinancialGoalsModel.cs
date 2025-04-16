using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GetGroupFinancialGoalsModel
    {
        [Required]
        [FromQuery(Name = "groupId")]
        public Guid GroupId { get; set; }
    }
}
