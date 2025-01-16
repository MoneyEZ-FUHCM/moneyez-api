using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.Controllers;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/subcategories")]
    [ApiController]
    public class SubCategoriesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubCategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add New SubCategory
        [HttpPost]
        public Task<IActionResult> AddSubCategory([FromBody] Subcategory subCategory)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid subcategory data."
                    };
                }

                await _unitOfWork.SubCategories.AddAsync(subCategory);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "SubCategory created successfully.",
                    Data = subCategory
                };
            });
        }

        // Get List SubCategories
        [HttpGet]
        public async Task<IActionResult> GetAllSubcategories([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            return await ValidateAndExecute(async () =>
            {
                var paginatedSubcategories = await _unitOfWork.SubCategories.GetPaginatedSubcategoriesAsync(pageIndex, pageSize);
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = paginatedSubcategories
                };
            });
        }
}
}
