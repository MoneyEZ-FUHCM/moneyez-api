using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class LimitBugdetSubcategoriesModel
    {
        public Guid SubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public decimal LimitBudget { get; set; }
    }
}
