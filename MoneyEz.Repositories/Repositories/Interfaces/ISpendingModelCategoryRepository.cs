using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ISpendingModelCategoryRepository : IGenericRepository<SpendingModelCategory>
    {
        Task<SpendingModelCategory?> GetByModelAndCategory(Guid spendingModelId, Guid categoryId);

    }
}
