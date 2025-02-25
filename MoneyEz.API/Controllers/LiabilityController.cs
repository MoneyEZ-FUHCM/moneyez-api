using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.LiabilityModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/liability")]
    [ApiController]
    public class LiabilityController : BaseController
    {
        private readonly ILiabilityService _liabilityService;

        public LiabilityController(ILiabilityService LiabilityService)
        {
            _liabilityService = LiabilityService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> GetAllLiabilitysPagination([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _liabilityService.GetAllLiabilitiesPaginationAsync(paginationParameter));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public Task<IActionResult> GetLiabilityById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _liabilityService.GetLiabilityByIdAsync(id));
        }

        [HttpGet]
        [Route("user")]
        [Authorize]
        public Task<IActionResult> GetLiabilitysByUser([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _liabilityService.GetLiabilitiesByUserAsync(paginationParameter));
        }

        [HttpPost]
        [Authorize]
        public Task<IActionResult> CreateLiability(CreateLiabilityModel model)
        {
            return ValidateAndExecute(() => _liabilityService.CreateLiabilityAsync(model));
        }

        [HttpPut]
        [Authorize]
        public Task<IActionResult> UpdateLiability(UpdateLiabilityModel model)
        {
            return ValidateAndExecute(() => _liabilityService.UpdateLiabilityAsync(model));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public Task<IActionResult> DeleteLiability([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _liabilityService.DeleteLiabilityAsync(id));
        }
    }
}
