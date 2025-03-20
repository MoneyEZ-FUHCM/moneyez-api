using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.BankAccountModels
{
    public class CreateBankAccountModel
    {
        [Required(ErrorMessage = "Account number is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Account number must be between 5 and 50 characters")]
        public string AccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Bank name is required")]
        [StringLength(250, ErrorMessage = "Bank name cannot exceed 250 characters")]
        public string BankName { get; set; } = null!;

        [Required(ErrorMessage = "Bank short name is required")]
        [StringLength(50, ErrorMessage = "Bank short name cannot exceed 50 characters")]
        public string BankShortName { get; set; } = null!;

        [Required(ErrorMessage = "Account holder name is required")]
        [StringLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters")]
        public string AccountHolderName { get; set; } = null!;
    }
}
