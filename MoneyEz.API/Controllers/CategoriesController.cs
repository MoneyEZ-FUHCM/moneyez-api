using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/categories")]
    [ApiController]
    [Authorize]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public Task<IActionResult> GetCategories([FromQuery] PaginationParameter paginationParameter, [FromQuery] CategoryFilter categoryFilter)
        {
            return ValidateAndExecute(() => _categoryService.GetCategoryPaginationAsync(paginationParameter, categoryFilter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetCategoryById(Guid id)
        {
            return ValidateAndExecute(() => _categoryService.GetCategoryByIdAsync(id));
        }

        /// <summary>
        /// Add list categories or one category.
        /// </summary>
        /// <param name="models">List categories will be added.</param>
        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPost]
        public Task<IActionResult> AddCategories([FromBody] List<CreateCategoryModel> models)
        {
            return ValidateAndExecute(() => _categoryService.AddCategoriesAsync(models));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPut]
        public Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryModel model)
        {
            return ValidateAndExecute(() => _categoryService.UpdateCategoryAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteCategory(Guid id)
        {
            return ValidateAndExecute(() => _categoryService.DeleteCategoryAsync(id));
        }
    }
}
