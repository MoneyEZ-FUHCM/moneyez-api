using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class UpdateGroupTransactionVoteModel
    {
        public Guid Id { get; set; }
        public bool Vote { get; set; }
    }
}
