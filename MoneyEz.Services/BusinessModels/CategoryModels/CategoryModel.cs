using MoneyEz.Repositories.Entities;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class CategoryModel:BaseEntity
    {
        public Guid Id { get; set; } // ID danh mục
        public string Name { get; set; } // Tên danh mục
        public string NameUnsign { get; set; } // Tên không dấu
        public Guid? ModelId { get; set; } // ID mô hình liên quan
        public string Description { get; set; } // Mô tả danh mục

        
      /*  public List<SubcategoryModel> Subcategories { get; set; } = new List<SubcategoryModel>();
        public List<SpendingModelCategoryModel> SpendingModelCategories { get; set; } = new List<SpendingModelCategoryModel>();*/
    }
}
