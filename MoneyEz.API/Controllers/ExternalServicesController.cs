using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/external-services")]
    [ApiController]
    public class ExternalServicesController : BaseController
    {
        private readonly IExternalApiService _externalApiService;

        public ExternalServicesController(IExternalApiService externalApiService)
        {
            _externalApiService = externalApiService;
        }

        [HttpPost]
        public Task<IActionResult> CreateTransactionPythonService(ExternalReciveRequestModel model)
        {
            return ValidateAndExecute(() => _externalApiService.ExecuteReceiveExternalService(model));
        }

        [HttpGet]
        public Task<IActionResult> GetChatMessageHistoriesPython([FromQuery] ExternalReciveRequestModel model)
        {
            return ValidateAndExecute(() => _externalApiService.ExecuteReceiveExternalService(model));
        }
    }
}
