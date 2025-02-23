using MoneyEz.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.BankAccountModels
{
    public class UpdateBankAccountModel : CreateBankAccountModel
    {
        [Required(ErrorMessage = "Bank account ID is required")]
        public Guid Id { get; set; }

        //public CommonsStatus? Status { get; set; }
    }
}
