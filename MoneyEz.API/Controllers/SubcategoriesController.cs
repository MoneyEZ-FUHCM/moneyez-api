using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/subcategories")]
    [ApiController]
    [Authorize]
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

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPost("create")]
        public Task<IActionResult> CreateSubcategories([FromBody] List<CreateSubcategoryModel> models)
        {
            return ValidateAndExecute(() => _subcategoryService.CreateSubcategoriesAsync(models));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPut("update")]
        public Task<IActionResult> UpdateSubcategoryById([FromBody] UpdateSubcategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.UpdateSubcategoryByIdAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPost("assign")]
        public Task<IActionResult> AddSubcategoriesToCategories([FromBody] AssignSubcategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.AddSubcategoriesToCategoriesAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
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
