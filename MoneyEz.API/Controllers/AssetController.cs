using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.AssetModels;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/asset")]
    [ApiController]
    public class AssetController : BaseController
    {
        private readonly IAssetService _assetService;

        [HttpGet]
        public Task<IActionResult> GetAllAssetsPagination([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _assetService.GetAllAssetsPaginationAsync(paginationParameter));
        }

        [HttpGet]
        [Route("{id}")]
        public Task<IActionResult> GetAssetById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _assetService.GetAssetByIdAsync(id));
        }

        [HttpGet]
        [Route("user/{userId}")]
        public Task<IActionResult> GetAssetsByUser([FromRoute] Guid userId, [FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _assetService.GetAssetsByUserAsync(userId, paginationParameter));
        }

        [HttpPost]
        public Task<IActionResult> CreateAsset(CreateAssetModel model)
        {
            return ValidateAndExecute(() => _assetService.CreateAssetAsync(model));
        }
    }
}
