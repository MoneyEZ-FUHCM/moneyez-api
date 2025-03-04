using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ISubcategoryService
    {
        Task<BaseResultModel> GetSubcategoriesPaginationAsync(PaginationParameter paginationParameter, SubcategoryFilter subcategoryFilter);
        Task<BaseResultModel> GetSubcategoryByIdAsync(Guid id);
        Task<BaseResultModel> CreateSubcategoriesAsync(List<CreateSubcategoryModel> models);
        Task<BaseResultModel> UpdateSubcategoryByIdAsync(UpdateSubcategoryModel model);
        Task<BaseResultModel> AddSubcategoriesToCategoriesAsync(AssignSubcategoryModel model);
        Task<BaseResultModel> DeleteSubcategoryAsync(Guid id);
        Task<BaseResultModel> RemoveSubcategoriesFromCategoriesAsync(RemoveSubcategoryFromCategoryModel model);
    }
}
