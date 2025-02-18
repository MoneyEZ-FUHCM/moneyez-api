using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/subcategories")]
    [ApiController]
    public class SubcategoriesController : BaseController
    {
        private readonly ISubcategoryService _subcategoryService;

        public SubcategoriesController(ISubcategoryService subcategoryService)
        {
            _subcategoryService = subcategoryService;
        }

        [HttpGet]
        public Task<IActionResult> GetSubcategories([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _subcategoryService.GetSubcategoriesPaginationAsync(paginationParameter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetSubcategoryById(Guid id)
        {
            return ValidateAndExecute(() => _subcategoryService.GetSubcategoryByIdAsync(id));
        }

        [HttpPost("create")]
        public Task<IActionResult> CreateSubcategories([FromBody] List<CreateSubcategoryModel> models)
        {
            return ValidateAndExecute(() => _subcategoryService.CreateSubcategoriesAsync(models));
        }

        [HttpPut("update")]
        public Task<IActionResult> UpdateSubcategoryById([FromBody] UpdateSubcategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.UpdateSubcategoryByIdAsync(model));
        }


        [HttpPost("assign")]
        public Task<IActionResult> AddSubcategoriesToCategories([FromBody] AssignSubcategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.AddSubcategoriesToCategoriesAsync(model));
        }

        [HttpDelete("remove")]
        public Task<IActionResult> RemoveSubcategoriesFromCategories([FromBody] RemoveSubcategoryFromCategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.RemoveSubcategoriesFromCategoriesAsync(model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteSubcategory(Guid id)
        {
            return ValidateAndExecute(() => _subcategoryService.DeleteSubcategoryAsync(id));
        }
    }
}
