using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class SpendingModelCategoryModel
    {
        public Guid? SpendingModelId { get; set; }

        public Guid? CategoryId { get; set; }

        public decimal? PercentageAmount { get; set; }

        public virtual CategoryModel? Category { get; set; }
        //public decimal PercentageAmount { get; set; } // Phần trăm phân bổ
        //public CategoryModel? Category { get; set; } // Thông tin chi tiết của Category
    }
}
