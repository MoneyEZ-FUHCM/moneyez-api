using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.AssetModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/asset")]
    [ApiController]
    public class AssetController : BaseController
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> GetAllAssetsPagination([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _assetService.GetAllAssetsPaginationAsync(paginationParameter));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public Task<IActionResult> GetAssetById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _assetService.GetAssetByIdAsync(id));
        }

        [HttpGet]
        [Route("user")]
        [Authorize]
        public Task<IActionResult> GetAssetsByUser([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _assetService.GetAssetsByUserAsync(paginationParameter));
        }

        [HttpPost]
        [Authorize]
        public Task<IActionResult> CreateAsset(CreateAssetModel model)
        {
            return ValidateAndExecute(() => _assetService.CreateAssetAsync(model));
        }

        [HttpPut]
        [Authorize]
        public Task<IActionResult> UpdateAsset(UpdateAssetModel model)
        {
            return ValidateAndExecute(() => _assetService.UpdateAssetAsync(model));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public Task<IActionResult> DeleteAsset([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _assetService.DeleteAssetAsync(id));
        }
    }
}
