using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ChartModels
{
    public class ChartSpendingCategoryModel
    {
        public string? CategoryName { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal PlannedPercentage { get; set; }
        public decimal ActualPercentage { get; set; }
    }
}
