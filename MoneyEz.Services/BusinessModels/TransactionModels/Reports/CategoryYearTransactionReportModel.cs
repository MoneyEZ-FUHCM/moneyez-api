using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Reports
{
    public class CategoryYearTransactionReportModel
    {
        public int Year { get; set; }
        public ReportTransactionType Type { get; set; }
        public decimal Total { get; set; }
        public List<CategoryAmountModel> Categories { get; set; } = new();
    }

    public class CategoryAmountModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string? Icon { get; set; }
    }
}