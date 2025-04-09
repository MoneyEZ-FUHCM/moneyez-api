using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Reports
{
    public class AllTimeTransactionSummaryModel
    {
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Total { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal Cumulation { get; set; }
    }
}
