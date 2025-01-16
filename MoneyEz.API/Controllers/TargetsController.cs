using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.Controllers;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/targets")]
    [ApiController]
    public class TargetsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TargetsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add Target for User
        [HttpPost("user")]
        public Task<IActionResult> AddTargetForUser([FromBody] Target target)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid target data."
                    };
                }

                await _unitOfWork.Targets.AddAsync(target);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Target created successfully.",
                    Data = target
                };
            });
        }

        // Add Target for Subcategories
        [HttpPost("subcategory")]
        public Task<IActionResult> AddTargetForSubcategories([FromBody] Target target)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid target data."
                    };
                }

                await _unitOfWork.Targets.AddAsync(target);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Target created for subcategory successfully.",
                    Data = target
                };
            });
        }

        // Get List of Targets with Pagination
        [HttpGet]
        public Task<IActionResult> GetTargets([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            return ValidateAndExecute(async () =>
            {
                var targets = await _unitOfWork.Targets.GetPaginatedTargetsAsync(pageIndex, pageSize);
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = targets
                };
            });
        }

        // Get Target by ID
        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetTargetById(Guid id)
        {
            return ValidateAndExecute(async () =>
            {
                var target = await _unitOfWork.Targets.GetTargetByIdAsync(id);

                if (target == null)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Target not found."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = target
                };
            });
        }
    }
}
