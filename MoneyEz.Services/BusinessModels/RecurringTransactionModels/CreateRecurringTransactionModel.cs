using MoneyEz.Repositories.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.RecurringTransactionModels
{
    public class CreateRecurringTransactionModel
    {
        [Required]
        public Guid SubcategoryId { get; set; }

        [Required]
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

        public CommonsStatus Status = CommonsStatus.ACTIVE; 
    }
}
