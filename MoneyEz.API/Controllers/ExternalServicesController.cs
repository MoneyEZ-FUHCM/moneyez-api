using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.QuizModels;
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
        public Task<IActionResult> RequestPostFromExternalService([FromBody] ExternalReciveRequestModel model)
        {
            return ValidateAndExecute(() => _externalApiService.ExecuteReceiveExternalService(model));
        }

        [HttpGet]
        public Task<IActionResult> RequestGetFromExternalService([FromQuery] ExternalReciveRequestModel model)
        {
            return ValidateAndExecute(() => _externalApiService.ExecuteReceiveExternalService(model));
        }

        [HttpPost("suggest-model/test")]
        public Task<IActionResult> SendToExternalService(List<QuestionAnswerPair> model)
        {
            return ValidateAndExecute(() => _externalApiService.SuggestionSpendingModelSerivceTest(model));
        }
    }
}
