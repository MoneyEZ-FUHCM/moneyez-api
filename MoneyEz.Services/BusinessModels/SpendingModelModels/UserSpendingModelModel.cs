using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class UserSpendingModelModel : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid SpendingModelId { get; set; }
        public PeriodUnit PeriodUnit { get; set; }
        public int PeriodValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }

        public SpendingModelModel SpendingModel { get; set; }
    }
}
