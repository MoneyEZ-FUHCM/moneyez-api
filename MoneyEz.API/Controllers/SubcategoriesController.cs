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

        [HttpPost]
        public Task<IActionResult> AddSubcategories([FromBody] List<CreateSubcategoryModel> models)
        {
            return ValidateAndExecute(() => _subcategoryService.AddSubcategoriesAsync(models));
        }

        [HttpPut]
        public Task<IActionResult> UpdateSubcategory([FromBody] UpdateSubcategoryModel model)
        {
            return ValidateAndExecute(() => _subcategoryService.UpdateSubcategoryAsync(model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteSubcategory(Guid id)
        {
            return ValidateAndExecute(() => _subcategoryService.DeleteSubcategoryAsync(id));
        }
    }
}
