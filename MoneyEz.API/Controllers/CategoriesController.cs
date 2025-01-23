using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/categories")] // Định nghĩa route API gốc cho module Category
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // API thêm mới danh mục
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryModel model)
        {
            var result = await _categoryService.AddCategoryAsync(model);
            return StatusCode(result.Status, result);
        }

        // API lấy danh sách danh mục (có phân trang)
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] PaginationParameter paginationParameter)
        {
            var result = await _categoryService.GetCategoriesAsync(paginationParameter);
            return StatusCode(result.Status, result);
        }

        // API lấy chi tiết danh mục theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryDetails(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return StatusCode(result.Status, result);
        }

        // API cập nhật thông tin danh mục
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryModel model)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, model);
            return StatusCode(result.Status, result);
        }

        // API xóa danh mục
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return StatusCode(result.Status, result);
        }
    }
}
