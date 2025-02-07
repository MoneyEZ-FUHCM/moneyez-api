using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("user/{userId}")]
        public Task<IActionResult> GetAllTransactions(Guid userId, [FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForUserAsync(userId, paginationParameter));
        }

        [HttpGet("{transactionId}")]
        public Task<IActionResult> GetTransactionById(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByIdAsync(transactionId));
        }

        [HttpPost]
        public Task<IActionResult> CreateTransaction([FromBody] CreateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateTransactionAsync(model));
        }

        [HttpPut]
        public Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionAsync(model));
        }

        [HttpDelete("{transactionId}")]
        public Task<IActionResult> DeleteTransaction(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteTransactionAsync(transactionId));
        }
    }
}
