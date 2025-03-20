using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class BankAccount : BaseEntity
    {
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string BankShortName { get; set; } = null!;
        public string AccountHolderName { get; set; } = null!;
        public CommonsStatus? Status { get; set; }
        public virtual User? User { get; set; }
        public string? WebhookSecretKey { get; set; }
        public string? WebhookUrl { get; set; }
    }
}
