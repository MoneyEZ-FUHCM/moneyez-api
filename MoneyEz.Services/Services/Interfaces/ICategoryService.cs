using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<BaseResultModel> GetCategoryPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetCategoryByIdAsync(Guid id);
        Task<BaseResultModel> AddCategoryAsync(CreateCategoryModel model);
        Task<BaseResultModel> UpdateCategoryAsync(Guid id, UpdateCategoryModel model);
        Task<BaseResultModel> DeleteCategoryAsync(Guid id);
    }
}
