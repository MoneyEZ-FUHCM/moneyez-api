using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.Controllers;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/categories")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add New Category
        [HttpPost]
        public Task<IActionResult> AddCategory([FromBody] Category category)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid category data."
                    };
                }

                await _unitOfWork.Categories.AddAsync(category);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Category created successfully.",
                    Data = category
                };
            });
        }

        // Get List Categories
        [HttpGet]
        public Task<IActionResult> GetListCategories()
        {
            return ValidateAndExecute(async () =>
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = categories
                };
            });
        }

        // Get Category Details
        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetCategoryDetails(Guid id)
        {
            return ValidateAndExecute(async () =>
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);

                if (category == null)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Category not found."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = category
                };
            });
        }
    }
}