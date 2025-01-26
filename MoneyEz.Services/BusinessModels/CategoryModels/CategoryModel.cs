using MoneyEz.Repositories.Entities;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class CategoryModel:BaseEntity
    {
        public string Name { get; set; }
        public string NameUnsign { get; set; } 
        public Guid? ModelId { get; set; }
        public string Description { get; set; }

    }
}
