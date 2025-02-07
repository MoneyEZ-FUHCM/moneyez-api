using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class FinancialHealthReport
    {
        public double SavingRatio { get; set; }
        public double ExpenseRatio { get; set; }
        public double DebtToIncomeRatio { get; set; }
        public double NetWorth { get; set; }
        public List<string> Suggestions { get; set; }
    }
}
