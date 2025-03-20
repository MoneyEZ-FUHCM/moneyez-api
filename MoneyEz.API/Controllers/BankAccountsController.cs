using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.WebhookModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/bank-accounts")]
    [ApiController]
    public class BankAccountsController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IWebhookService _webhookService;

        public BankAccountsController(IBankAccountService bankAccountService, IWebhookService webhookService)
        {
            _bankAccountService = bankAccountService;
            _webhookService = webhookService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> GetAllBankAccounts([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _bankAccountService.GetAllBankAccountsPaginationAsync(paginationParameter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public Task<IActionResult> GetBankAccountById(Guid id)
        {
            return ValidateAndExecute(() => _bankAccountService.GetBankAccountByIdAsync(id));
        }

        [HttpGet("user")]
        [Authorize]
        public Task<IActionResult> GetBankAccountsByUser([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _bankAccountService.GetBankAccountsByUserAsync(paginationParameter));
        }

        [HttpPost]
        [Authorize]
        public Task<IActionResult> CreateBankAccount([FromBody] CreateBankAccountModel model)
        {
            return ValidateAndExecute(() => _bankAccountService.CreateBankAccountAsync(model));
        }

        [HttpPut]
        [Authorize]
        public Task<IActionResult> UpdateBankAccount([FromBody] UpdateBankAccountModel model)
        {
            return ValidateAndExecute(() => _bankAccountService.UpdateBankAccountAsync(model));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public Task<IActionResult> DeleteBankAccount(Guid id)
        {
            return ValidateAndExecute(() => _bankAccountService.DeleteBankAccountAsync(id));
        }

        [HttpPost("register-webhook")]
        [Authorize]
        public Task<IActionResult> RegisterWebhook([FromBody] WebhookRegisterModel model)
        {
            return ValidateAndExecute(() => _webhookService.RegisterWebhookAsync(model.AccountBankId));
        }
    }
}
