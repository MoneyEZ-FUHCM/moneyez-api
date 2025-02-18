using MoneyEz.Repositories.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class UpdateTransactionModel
    {
        [Required(ErrorMessage = "Transaction ID is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Subcategory ID is required.")]
        public Guid SubcategoryId { get; set; }

        [Required(ErrorMessage = "Transaction date is required.")]
        public DateTime TransactionDate { get; set; }

        public string Description { get; set; }
        public bool? ApprovalRequired { get; set; }
    }
}
