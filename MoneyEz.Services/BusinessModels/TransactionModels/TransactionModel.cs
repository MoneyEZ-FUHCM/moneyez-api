using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class TransactionModel : BaseEntity
    {
        public Guid? UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Type { get; set; }
        public Guid? UserSpendingModelId { get; set; }
        public Guid? SubcategoryId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? SubcategoryName { get; set; }
        public string? SubcategoryIcon { get; set; }
        public List<string>? Images { get; set; }
    }
}
