using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class CategorySubcategoryRepository : GenericRepository<CategorySubcategory>, ICategorySubcategoryRepository
    {
        public CategorySubcategoryRepository(MoneyEzContext context) : base(context)
        {
        }
    }
}
