using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.BankAccountModels;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class FundraisingTransactionResponse
    {
        public string RequestCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public BankAccountModel? BankAccount { get; set; } = null!;
    }
}
