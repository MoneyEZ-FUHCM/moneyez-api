using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Group
{
    public class GroupTransactionModel : BaseEntity
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public List<string>? Images { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.PENDING;
        public bool ApprovalRequired { get; set; }
        public string? RequestCode { get; set; }
        public string? InsertType { get; set; }
    }
}
