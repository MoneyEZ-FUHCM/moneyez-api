using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/categories")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public Task<IActionResult> GetCategories([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _categoryService.GetCategoryPaginationAsync(paginationParameter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetCategoryById(Guid id)
        {
            return ValidateAndExecute(() => _categoryService.GetCategoryByIdAsync(id));
        }

        [HttpPost]
        public Task<IActionResult> AddCategory([FromBody] CreateCategoryModel model)
        {
            return ValidateAndExecute(() => _categoryService.AddCategoryAsync(model));
        }

        [HttpPut("{id}")]
        public Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryModel model)
        {
            return ValidateAndExecute(() => _categoryService.UpdateCategoryAsync(id, model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteCategory(Guid id)
        {
            return ValidateAndExecute(() => _categoryService.DeleteCategoryAsync(id));
        }

        [HttpPost("bulk")]
        public Task<IActionResult> AddListCategories([FromBody] List<CreateCategoryModel> models)
        {
            return ValidateAndExecute(() => _categoryService.AddListCategoriesAsync(models));
        }
    }
}
