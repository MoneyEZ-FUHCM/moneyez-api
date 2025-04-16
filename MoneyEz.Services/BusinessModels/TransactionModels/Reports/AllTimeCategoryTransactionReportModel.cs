using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Reports
{
    public class AllTimeCategoryTransactionReportModel
    {
        public string? Type { get; set; }
        public decimal Total { get; set; }
        public List<CategoryAmountModel> Categories { get; set; } = new();
    }
}
