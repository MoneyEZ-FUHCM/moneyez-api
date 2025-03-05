using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ICategorySubcategoryRepository : IGenericRepository<CategorySubcategory>
    {
        Task<List<Subcategory>> GetSubcategoriesBySpendingModelId(Guid spendingModelId);
        Task<Category?> GetCategoryBySubcategoryId(Guid subcategoryId);
    }

}
