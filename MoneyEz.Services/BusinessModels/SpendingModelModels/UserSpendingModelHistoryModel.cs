using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class UserSpendingModelHistoryModel : BaseEntity
    {
        public Guid SpendingModelId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsTemplate { get; set; }
        public PeriodUnit PeriodUnit { get; set; }
        public int PeriodValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public string? Status { get; set; }
    }
}
