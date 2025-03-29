using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.RecurringTransactionModels
{
    public class UpdateRecurringTransactionModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid SubcategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public FrequencyType FrequencyType { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Description { get; set; }

        public string Tags { get; set; }
    }
}
