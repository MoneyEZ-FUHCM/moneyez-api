using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class GroupTransactionVoteModel : BaseEntity
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public bool Vote { get; set; }
    }
}
