using MoneyEz.Services.BusinessModels.CategoryModels;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class SpendingModelCategoryModel
    {
        public decimal PercentageAmount { get; set; } // Phần trăm phân bổ
        public CategoryModel Category { get; set; } // Thông tin chi tiết của Category
    }
}
