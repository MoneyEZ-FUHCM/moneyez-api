using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/spending-models")]
    [ApiController]
    [Authorize(Roles = nameof(RolesEnum.ADMIN))]
    public class SpendingModelsController : BaseController
    {
        private readonly ISpendingModelService _spendingModelService;

        public SpendingModelsController(ISpendingModelService spendingModelService)
        {
            _spendingModelService = spendingModelService;
        }

        [HttpGet]
        public Task<IActionResult> GetSpendingModels([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _spendingModelService.GetSpendingModelsPaginationAsync(paginationParameter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetSpendingModelById(Guid id)
        {
            return ValidateAndExecute(() => _spendingModelService.GetSpendingModelByIdAsync(id));
        }

        [HttpPost]
        public Task<IActionResult> AddSpendingModel([FromBody] List<CreateSpendingModelModel> models)
        {
            return ValidateAndExecute(() => _spendingModelService.AddSpendingModelsAsync(models));
        }

        [HttpPut]
        public Task<IActionResult> UpdateSpendingModel([FromBody] UpdateSpendingModelModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.UpdateSpendingModelAsync(model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteSpendingModel(Guid id)
        {
            return ValidateAndExecute(() => _spendingModelService.DeleteSpendingModelAsync(id));
        }

        [HttpPost("categories")]
        public Task<IActionResult> AddCategoriesToSpendingModel([FromBody] AddCategoriesToSpendingModelModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.AddCategoriesToSpendingModelAsync(model));
        }

        [HttpPut("categories")]
        public Task<IActionResult> UpdateCategoryPercentage([FromBody] UpdateCategoryPercentageModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.UpdateCategoryPercentageAsync(model));
        }

        [HttpDelete("categories")]
        public Task<IActionResult> RemoveCategoriesFromSpendingModel([FromBody] RemoveCategoriesFromSpendingModelModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.RemoveCategoriesFromSpendingModelAsync(model));
        }
    }
}
