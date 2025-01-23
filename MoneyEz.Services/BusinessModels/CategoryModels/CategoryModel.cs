namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class CategoryModel
    {
        public Guid Id { get; set; } // ID danh mục
        public string Name { get; set; } // Tên danh mục
        public string NameUnsign { get; set; } // Tên không dấu
        public Guid? ModelId { get; set; } // ID mô hình liên quan
        public string Description { get; set; } // Mô tả danh mục

        // Thuộc tính từ BaseEntity
        public DateTime CreatedDate { get; set; } // Ngày tạo
        public string CreatedBy { get; set; } // Người tạo
        public DateTime? UpdatedDate { get; set; } // Ngày cập nhật
        public string UpdatedBy { get; set; } // Người cập nhật
        public bool IsDeleted { get; set; } // Trạng thái xóa mềm

        
      /*  public List<SubcategoryModel> Subcategories { get; set; } = new List<SubcategoryModel>();
        public List<SpendingModelCategoryModel> SpendingModelCategories { get; set; } = new List<SpendingModelCategoryModel>();*/
    }
}
