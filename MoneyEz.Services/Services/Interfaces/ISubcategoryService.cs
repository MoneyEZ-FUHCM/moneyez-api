using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ISubcategoryService
    {
        Task<BaseResultModel> GetSubcategoriesPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetSubcategoryByIdAsync(Guid id);
        Task<BaseResultModel> AddSubcategoriesAsync(List<CreateSubcategoryModel> models);
        Task<BaseResultModel> UpdateSubcategoryAsync(UpdateSubcategoryModel model);
        Task<BaseResultModel> DeleteSubcategoryAsync(Guid id);
    }
}
