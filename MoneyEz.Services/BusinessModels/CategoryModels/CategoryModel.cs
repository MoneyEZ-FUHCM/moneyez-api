using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.SubcategoryModels;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class CategoryModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? NameUnsign { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string? Icon { get; set; }
        public string? Type { get; set; }
        public List<SubcategoryModel> Subcategories { get; set; } = new List<SubcategoryModel>();
    }
}
