﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("user")]
        public Task<IActionResult> GetAllTransactions([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForUserAsync(paginationParameter));
        }
        
        [HttpGet("user-spending-model/{userSpendingModelId}")]
        public Task<IActionResult> GetAllTransactionsBySpendingModel([FromQuery] PaginationParameter paginationParameter, Guid userSpendingModelId)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionsByUserSpendingModelAsync(paginationParameter, userSpendingModelId));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("{transactionId}")]
        public Task<IActionResult> GetTransactionById(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByIdAsync(transactionId));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPost]
        public Task<IActionResult> CreateTransaction([FromBody] CreateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateTransactionAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPut]
        public Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpDelete("{transactionId}")]
        public Task<IActionResult> DeleteTransaction(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteTransactionAsync(transactionId));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("admin")]
        public Task<IActionResult> GetAllTransactionsForAdmin([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForAdminAsync(paginationParameter));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("groups")]
        [Authorize]
        public Task<IActionResult> GetAllTransactionsForGroup([FromQuery] PaginationParameter paginationParameter, [FromQuery] TransactionFilter filter)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByGroupIdAsync(paginationParameter, filter));
        }

    }
}
