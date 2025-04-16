using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Reports
{
    public class BalanceYearTransactionReportModel
    {
        public int Year { get; set; }
        public List<MonthlyBalanceModel> Balances { get; set; } = new();
    }

    public class MonthlyBalanceModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

}