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

        // ** User Transactions **
        [HttpPost("user")]
        public Task<IActionResult> CreateTransactionForUser([FromBody] CreateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateTransactionForUserAsync(model));
        }

        [HttpPut("user")]
        public Task<IActionResult> UpdateTransactionForUser([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionForUserAsync(model));
        }

        [HttpGet("user")]
        public Task<IActionResult> GetUserTransactions([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetUserTransactionsAsync(paginationParameter));
        }

        [HttpGet("user/{id}")]
        public Task<IActionResult> GetUserTransactionById(Guid id)
        {
            return ValidateAndExecute(() => _transactionService.GetUserTransactionByIdAsync(id));
        }

        [HttpDelete("user")]
        public Task<IActionResult> RemoveUserTransaction([FromBody] DeleteTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.RemoveUserTransactionAsync(model.Id));
        }

        // ** Group Transactions **
        [HttpPost("group")]
        public Task<IActionResult> CreateTransactionForGroup([FromBody] CreateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateTransactionForGroupAsync(model));
        }

        [HttpPut("group")]
        public Task<IActionResult> UpdateTransactionForGroup([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionForGroupAsync(model));
        }

        [HttpGet("group")]
        public Task<IActionResult> GetGroupTransactions([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetGroupTransactionsAsync(paginationParameter));
        }

        [HttpGet("group/user/{userId}")]
        public Task<IActionResult> GetGroupTransactionByUserId(Guid userId)
        {
            return ValidateAndExecute(() => _transactionService.GetGroupTransactionByUserIdAsync(userId));
        }

        [HttpDelete("group")]
        public Task<IActionResult> RemoveGroupTransaction([FromBody] DeleteTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.RemoveGroupTransactionAsync(model.Id));
        }

        // ** Approval Handling **
        [HttpPut("approve")]
        public Task<IActionResult> ApproveTransaction([FromBody] TransactionApprovalModel model)
        {
            return ValidateAndExecute(() => _transactionService.ApproveTransactionAsync(model.Id));
        }

        [HttpPut("reject")]
        public Task<IActionResult> RejectTransaction([FromBody] TransactionApprovalModel model)
        {
            return ValidateAndExecute(() => _transactionService.RejectTransactionAsync(model.Id));
        }
    }
}
