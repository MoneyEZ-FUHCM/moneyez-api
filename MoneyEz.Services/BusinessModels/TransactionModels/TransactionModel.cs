using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using MoneyEz.Services.BusinessModels.UserModels;
using System;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class TransactionModel : BaseEntity
    {
        public Guid? GroupId { get; set; }
        public Guid? UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public Guid? SubcategoryId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public bool? ApprovalRequired { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
