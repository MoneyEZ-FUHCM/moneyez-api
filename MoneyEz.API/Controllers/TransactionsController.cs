using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.BusinessModels.WebhookModels;
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

        #region single user
        [HttpGet("user")]
        [Authorize]
        public Task<IActionResult> GetAllTransactions([FromQuery] PaginationParameter paginationParameter, [FromQuery] TransactionFilter filter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForUserAsync(paginationParameter, filter));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("user/{transactionId}")]
        public Task<IActionResult> GetTransactionById(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByIdAsync(transactionId));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPost("user")]
        public Task<IActionResult> CreateTransaction([FromBody] CreateTransactionModel model)
        {
            var currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _transactionService.CreateTransactionAsync(model, currentEmail));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpPut("user")]
        public Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpDelete("user/{transactionId}")]
        public Task<IActionResult> DeleteTransaction(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteTransactionAsync(transactionId));
        }
        #endregion single user

        #region admin   
        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("admin")]
        public Task<IActionResult> GetAllTransactionsForAdmin([FromQuery] PaginationParameter paginationParameter, [FromQuery] TransactionFilter filter)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTransactionsForAdminAsync(paginationParameter, filter));
        }
        #endregion admin

        #region group
        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("groups")]
        [Authorize]
        public Task<IActionResult> GetAllTransactionsForGroupByGroupID([FromQuery] Guid groupId, 
            [FromQuery] PaginationParameter paginationParameter, [FromQuery] TransactionFilter filter)
        {
            return ValidateAndExecute(() => _transactionService.GetTransactionByGroupIdAsync(groupId, paginationParameter, filter));
        }

        [HttpPost("group/create")]
        public Task<IActionResult> CreateGroupTransaction([FromBody] CreateGroupTransactionModel model)
        {
            var currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _transactionService.CreateGroupTransactionAsync(model, currentEmail));
        }

        [HttpGet("group/transaction/{transactionId}")]
        public Task<IActionResult> GetGroupTransactionDetails(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.GetGroupTransactionDetailsAsync(transactionId));
        }

        [HttpPut("group/update")]
        public Task<IActionResult> UpdateGroupTransaction([FromBody] UpdateGroupTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateGroupTransactionAsync(model));
        }

        [HttpDelete("group/delete/{transactionId}")]
        public Task<IActionResult> DeleteGroupTransaction(Guid transactionId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteGroupTransactionAsync(transactionId));
        }

        [HttpPost("group/response")]
        public Task<IActionResult> ResponseGroupTransaction(ResponseGroupTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.ResponseGroupTransactionAsync(model));
        }

        #region vote

        [HttpPost("group/vote/create")]
        public Task<IActionResult> CreateGroupTransactionVote([FromBody] CreateGroupTransactionVoteModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateGroupTransactionVoteAsync(model));
        }

        [HttpPut("group/vote/update")]
        public Task<IActionResult> UpdateGroupTransactionVote([FromBody] UpdateGroupTransactionVoteModel model)
        {
            return ValidateAndExecute(() => _transactionService.UpdateGroupTransactionVoteAsync(model));
        }

        [HttpDelete("group/vote/delete/{voteId}")]
        public Task<IActionResult> DeleteGroupTransactionVote(Guid voteId)
        {
            return ValidateAndExecute(() => _transactionService.DeleteGroupTransactionVoteAsync(voteId));
        }

        #endregion vote

        #endregion group

        #region duc
        [HttpPost("webhook")]
        public Task<IActionResult> UpdateTransactionWebhook(WebhookPayload webhookPayload)
        {
            return ValidateAndExecute(() => _transactionService.UpdateTransactionWebhook(webhookPayload));
        }

        [HttpPost("python-service")]
        public Task<IActionResult> CreateTransactionPythonService(CreateTransactionPythonModel model)
        {
            return ValidateAndExecute(() => _transactionService.CreateTransactionPythonService(model));
        }

        [HttpPut("categorize")]
        public Task<IActionResult> CategorizeTransaction(CategorizeTransactionModel model)
        {
            return ValidateAndExecute(() => _transactionService.CategorizeTransactionAsync(model));
        }
        #endregion duc

        #region report

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/year")]
        public Task<IActionResult> GetYearReport([FromQuery] int year, [FromQuery] ReportTransactionType type = ReportTransactionType.EXPENSE)
        {
            return ValidateAndExecute(() => _transactionService.GetYearReportAsync(year, type));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/category-year")]
        public Task<IActionResult> GetCategoryYearReport([FromQuery] int year, [FromQuery] ReportTransactionType type = ReportTransactionType.EXPENSE)
        {
            return ValidateAndExecute(() => _transactionService.GetCategoryYearReportAsync(year, type));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/all-time")]
        public Task<IActionResult> GetAllTimeReport()
        {
            return ValidateAndExecute(() => _transactionService.GetAllTimeReportAsync());
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/all-time-category")]
        public Task<IActionResult> GetAllTimeCategoryReport([FromQuery] ReportTransactionType type = ReportTransactionType.EXPENSE)
        {
            return ValidateAndExecute(() => _transactionService.GetAllTimeCategoryReportAsync(type));
        }

        [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/balance-year")]
        public Task<IActionResult> GetBalanceYearReport([FromQuery] int year)
        {
            return ValidateAndExecute(() => _transactionService.GetBalanceYearReportAsync(year));
        }

        /*  [Authorize(Roles = nameof(RolesEnum.USER))]
        [HttpGet("report/category-year-v2")]
        public Task<IActionResult> GetCategoryYearReportV2([FromQuery] int year, [FromQuery] string type)
        {
            return ValidateAndExecute(() => _transactionService.GetCategoryYearReportAsyncV2(year, type));
        }*/

        #endregion report
    }
}
