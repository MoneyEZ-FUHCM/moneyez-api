using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.RecurringTransactionModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/recurring-transactions")]
    [ApiController]
    public class RecurringTransactionsController : BaseController
    {
        private readonly IRecurringTransactionService _recurringTransactionService;

        public RecurringTransactionsController(IRecurringTransactionService recurringTransactionService)
        {
            _recurringTransactionService = recurringTransactionService;
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPost]
        public Task<IActionResult> AddRecurringTransaction([FromBody] CreateRecurringTransactionModel model)
        {
            return ValidateAndExecute(() => _recurringTransactionService.AddRecurringTransactionAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet]
        public Task<IActionResult> GetAllRecurringTransactions([FromQuery] PaginationParameter paginationParameter, [FromQuery] RecurringTransactionFilter filter)
        {
            return ValidateAndExecute(() => _recurringTransactionService.GetAllRecurringTransactionsAsync(paginationParameter, filter));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("{recurringTransactionId}")]
        public Task<IActionResult> GetRecurringTransactionById(Guid recurringTransactionId)
        {
            return ValidateAndExecute(() => _recurringTransactionService.GetRecurringTransactionByIdAsync(recurringTransactionId));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPut]
        public Task<IActionResult> UpdateRecurringTransaction([FromBody] UpdateRecurringTransactionModel model)
        {
            return ValidateAndExecute(() => _recurringTransactionService.UpdateRecurringTransactionAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpDelete("{recurringTransactionId}")]
        public Task<IActionResult> DeleteRecurringTransaction(Guid recurringTransactionId)
        {
            return ValidateAndExecute(() => _recurringTransactionService.DeleteRecurringTransactionAsync(recurringTransactionId));
        }
    }
}
