using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/user-spending-models")]
    [ApiController]
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
        public Task<IActionResult> CancelSpendingModel([FromQuery] Guid spendingModelId, [FromQuery] bool isBypassGoal, [FromQuery] bool isBypassTransaction)
        {
            return ValidateAndExecute(() => _userSpendingModelService.CancelSpendingModelAsync(spendingModelId, isBypassGoal, isBypassTransaction));
        }

        [HttpGet("current")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetCurrentSpendingModel()
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetCurrentSpendingModelAsync());
        }

        [HttpGet("current/chart")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetChartCurrentSpendingModel()
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetChartCurrentSpendingModelAsync());
        }

        [HttpGet("current/categories")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetCategoriesCurrentSpendingModel()
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetCategoriesCurrentSpendingModelAsync());
        }

        [HttpGet("current/sub-categories")]
        [Authorize(Roles = nameof(RolesEnum.USER))]
        public Task<IActionResult> GetSubCategoriesCurrentSpendingModel([FromQuery] CategoryCurrentSpendingModelFiter fiter)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetSubCategoriesCurrentSpendingModelAsync(fiter));
        }

        [HttpGet("chart/{id}")]
        [Authorize]
        public Task<IActionResult> GetChartSpendingModel(Guid id)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetChartSpendingModelAsync(id));
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


        [HttpGet("transactions/{id}")]
        [Authorize]
        public Task<IActionResult> GetAllTransactionsBySpendingModel([FromQuery] PaginationParameter paginationParameter, [FromQuery] TransactionFilter transactionFilter, Guid id)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetTransactionsByUserSpendingModelAsync(paginationParameter, transactionFilter, id));
        }

        [HttpGet("scan")]
        public Task<IActionResult> ScanUserSpendingModelTimeExpried()
        {
            return ValidateAndExecute(() => _userSpendingModelService.ProcessExpiredAndUpcomingSpendingModelsAsync());
        }

        [HttpGet("current/webhook/sub-categories")]
        public Task<IActionResult> GetSubCategoriesCurrentSpendingModelUserId([FromQuery] Guid userId)
        {
            return ValidateAndExecute(() => _userSpendingModelService.GetSubCategoriesCurrentSpendingModelByUserIdAsync(userId));
        }
    }
}
