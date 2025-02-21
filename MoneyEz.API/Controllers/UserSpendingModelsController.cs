using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/user-spending-models")]
    [ApiController]
    [Authorize]
    public class UserSpendingModelsController : BaseController
    {
        private readonly IUserSpendingModelService _userSpendingModelService;

        public UserSpendingModelsController(IUserSpendingModelService userSpendingModelService)
        {
            _userSpendingModelService = userSpendingModelService;
        }

        [HttpPost("choose")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> ChooseSpendingModel([FromBody] ChooseSpendingModelModel model)
        {
            return ValidateAndExecute(() => _userSpendingModelService.ChooseSpendingModelAsync(model));
        }

        [HttpPost("switch")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> SwitchSpendingModel([FromBody] SwitchSpendingModelModel model)
        {
            return ValidateAndExecute(() => _userSpendingModelService.SwitchSpendingModelAsync(model));
        }

        [HttpDelete("cancel")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> CancelSpendingModel([FromQuery] Guid spendingModelId)
        {
            return ValidateAndExecute(() => _userSpendingModelService.CancelSpendingModelAsync(spendingModelId));
        }

        [HttpGet("current")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetCurrentSpendingModel()
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetCurrentSpendingModelAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetUsedSpendingModelById(Guid id)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetUsedSpendingModelByIdAsync(id));
        }

        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetUsedSpendingModels([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetUsedSpendingModelsPaginationAsync(paginationParameter));
        }
    }
}
