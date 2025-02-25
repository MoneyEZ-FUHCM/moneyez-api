using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class UpdateGroupTransactionModel
    {
        public Guid GroupId { get; set; }

        public Guid TransactionId { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
