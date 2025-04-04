using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Group
{
    public class ResponseGroupTransactionModel
    {
        public Guid TransactionId { get; set; }

        public bool IsApprove { get; set; } = false;

        public string? Note { get; set; } = string.Empty;
    }
}
