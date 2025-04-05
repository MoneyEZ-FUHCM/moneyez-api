using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.WebhookModels
{
    public class WebhookPayload
    {
        public string AccountNumber { get; set; } = "";
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Timestamp { get; set; }
        public string TransactionId { get; set; } = default!;
        public string Description { get; set; } = "";
        public string BankName { get; set; } = "";
    }
}
