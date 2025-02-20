using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.Services.Implements;
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
        private readonly IClaimsService _claimsService;

        public TransactionsController(ITransactionService transactionService, IClaimsService claimsService)
        {
            _transactionService = transactionService;
            _claimsService = claimsService;
        }

        [HttpGet("user")]
        [Authorize]
        public Task<IActionResult> GetAllTransactions([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForUserAsync(paginationParameter));
        }

        [HttpGet("{transactionId}")]
        [Authorize]
        public Task<IActionResult> GetTransactionById(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByIdAsync(transactionId));
        }

        [HttpPost]
        [Authorize]
        public Task<IActionResult> CreateTransaction([FromBody] CreateTransactionModel model)
        {
            var currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _transactionService.CreateTransactionAsync(model, currentEmail));
        }

        [HttpPut]
        [Authorize]
        public Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionAsync(model));
        }

        [HttpDelete("{transactionId}")]
        [Authorize]
        public Task<IActionResult> DeleteTransaction(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteTransactionAsync(transactionId));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> GetAllTransactionsForAdmin([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForAdminAsync(paginationParameter));
        }

    }
}
