using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<BaseResultModel> GetCategoryPaginationAsync(PaginationParameter paginationParameter, CategoryFilter categoryFilter);
        Task<BaseResultModel> GetCategoryByIdAsync(Guid id);
        Task<BaseResultModel> AddCategoriesAsync(List<CreateCategoryModel> models);
        Task<BaseResultModel> UpdateCategoryAsync(UpdateCategoryModel model);
        Task<BaseResultModel> DeleteCategoryAsync(Guid id);
    }
}