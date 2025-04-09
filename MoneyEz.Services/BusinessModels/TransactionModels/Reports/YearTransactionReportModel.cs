using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Reports
{
    public class YearTransactionReportModel
    {
        public int Year { get; set; }
        public ReportTransactionType Type { get; set; }
        public decimal Total { get; set; }
        public decimal Average { get; set; }
        public List<MonthAmountModel> MonthlyData { get; set; } = new();
    }

    public class MonthAmountModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
