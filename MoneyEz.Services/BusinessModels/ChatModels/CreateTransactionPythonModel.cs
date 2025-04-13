using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ChatModels
{
    public class CreateTransactionPythonModel
    {
        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Subcategory code is required.")]
        public string SubcategoryCode { get; set; } = "";

        public string? Description { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }
    }

    public class CreateTransactionPythonModelV2
    {
        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Subcategory code is required.")]
        public string SubcategoryCode { get; set; } = "";

        public string? Description { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
