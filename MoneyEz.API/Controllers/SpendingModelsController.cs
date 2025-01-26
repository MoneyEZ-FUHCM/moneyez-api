using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/spendingmodels")]
    [ApiController]
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

        [HttpPut("{id}")]
        public Task<IActionResult> UpdateSpendingModel(Guid id, [FromBody] UpdateSpendingModelModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.UpdateSpendingModelAsync(id, model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteSpendingModel(Guid id)
        {
            return ValidateAndExecute(() => _spendingModelService.DeleteSpendingModelAsync(id));
        }

        [HttpPost("{id}/categories")]
        public Task<IActionResult> AddCategoriesToSpendingModel(Guid id, [FromBody] AddCategoriesToSpendingModelModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.AddCategoriesToSpendingModelAsync(id, model));
        }

        [HttpPut("{id}/categories")]
        public Task<IActionResult> UpdateCategoryPercentage(Guid id, [FromBody] UpdateCategoryPercentageModel model)
        {
            return ValidateAndExecute(() => _spendingModelService.UpdateCategoryPercentageAsync(id, model));
        }

        [HttpDelete("{id}/categories")]
        public Task<IActionResult> RemoveCategoriesFromSpendingModel(Guid id, [FromBody] List<Guid> categoryIds)
        {
            return ValidateAndExecute(() => _spendingModelService.RemoveCategoriesFromSpendingModelAsync(id, categoryIds));
        }
    }
}
