using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<BaseResultModel> AddCategoryAsync(CreateCategoryModel model); // Thêm mới danh mục
        Task<BaseResultModel> GetCategoriesAsync(PaginationParameter paginationParameter); // Lấy danh sách danh mục (có phân trang)
        Task<BaseResultModel> GetCategoryByIdAsync(Guid id); // Lấy thông tin chi tiết danh mục theo ID
        Task<BaseResultModel> UpdateCategoryAsync(Guid id, UpdateCategoryModel model); // Cập nhật danh mục
        Task<BaseResultModel> DeleteCategoryAsync(Guid id); // Xóa danh mục
    }
}
