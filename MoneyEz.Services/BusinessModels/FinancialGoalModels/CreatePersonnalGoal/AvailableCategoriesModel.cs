using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels.CreatePersonnalGoal
{
    public class AvailableCategoriesModel
    {
        public Guid CategoryId { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public List<AvailableSubcategoriesModel> Subcategories { get; set; } = new List<AvailableSubcategoriesModel>();
    }

    public class AvailableSubcategoriesModel
    {
        public Guid SubcategoryId { get; set; }
        public string? SubcategoryCode { get; set; }
        public string? SubcategoryName { get; set; }
        public string? SubcategoryIcon { get; set; }
        public string? Status { get; set; }
    }
}
