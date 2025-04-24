using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class FundraisingTransactionResponse
    {
        //public string RequestCode { get; set; } = null!;
        //public decimal Amount { get; set; }
        //public string Status { get; set; } = "";
        public GroupTransactionModel? Transaction { get; set; }
        public BankAccountModel? BankAccount { get; set; } = null!;
    }
}
