using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class ApproveGroupFinancialGoalRequestModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid GoalId { get; set; }

        [Required]
        public bool IsApproved { get; set; } // true = duyệt, false = từ chối

        public string? RejectionReason { get; set; }

        [Required]
        public string ActionType { get; set; } // CREATE/UPDATE/DELETE
    }

}
