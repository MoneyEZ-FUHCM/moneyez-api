using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class TransactionHistorySendToPython
    {
        public string? Description { get; set; }
        public string? SubcategoryName { get; set; }
        public string? Type { get; set; }
        public decimal? Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
