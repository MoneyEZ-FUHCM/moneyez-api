using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class CancelSpendingModelModel
    {
        [Required(ErrorMessage = "Spending Model Id is required.")]
        public Guid SpendingModelId { get; set; }
    }
}
