using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.BankAccountModels
{
    public class BankAccountModel : BaseEntity
    {
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string BankShortName { get; set; } = null!;
        public string AccountHolderName { get; set; } = null!;
        public CommonsStatus? Status { get; set; }
        public bool IsLinked { get; set; } = false;
        public bool IsHasGroup { get; set; } = false;
    }
}
