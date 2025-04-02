using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Group
{
    public class DeleteGroupTransactionModel
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
    }
}
