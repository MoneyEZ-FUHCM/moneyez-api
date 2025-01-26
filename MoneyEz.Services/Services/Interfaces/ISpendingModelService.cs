using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SpendingModelModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ISpendingModelService
    {
        Task<BaseResultModel> GetSpendingModelsPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetSpendingModelByIdAsync(Guid id);
        Task<BaseResultModel> AddSpendingModelsAsync(List<CreateSpendingModelModel> models);
        Task<BaseResultModel> UpdateSpendingModelAsync(Guid id, UpdateSpendingModelModel model);
        Task<BaseResultModel> DeleteSpendingModelAsync(Guid id);
        Task<BaseResultModel> AddCategoriesToSpendingModelAsync(Guid spendingModelId, AddCategoriesToSpendingModelModel model);
        Task<BaseResultModel> UpdateCategoryPercentageAsync(Guid id, UpdateCategoryPercentageModel model);
        Task<BaseResultModel> RemoveCategoriesFromSpendingModelAsync(Guid id, List<Guid> categoryIds);
    }
}
