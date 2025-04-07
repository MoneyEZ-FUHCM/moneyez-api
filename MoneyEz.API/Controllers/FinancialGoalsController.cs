using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/financial-goals")]
    [ApiController]
    [Authorize(Roles = nameof(RolesEnum.USER))]
    public class FinancialGoalsController : BaseController
    {
        private readonly IFinancialGoalService _financialGoalService;

        public FinancialGoalsController(IFinancialGoalService financialGoalService)
        {
            _financialGoalService = financialGoalService;
        }

        #region Personal Financial Goals

        [HttpPost("personal")]

        public Task<IActionResult> AddPersonalFinancialGoal([FromBody] AddPersonalFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.AddPersonalFinancialGoalAsync(model));
        }

        [HttpGet("personal")]

        public Task<IActionResult> GetPersonalFinancialGoals([FromQuery] PaginationParameter paginationParameter, [FromQuery] FinancialGoalFilter filter)
        {
            return ValidateAndExecute(() => _financialGoalService.GetPersonalFinancialGoalsAsync(paginationParameter, filter));
        }

        [HttpGet("personal/limit-budget/subcategory/{subcategoryId}")]

        public Task<IActionResult> GetUserLimitBugdetSubcategoryAsync(Guid subcategoryId)
        {
            return ValidateAndExecute(() => _financialGoalService.GetUserLimitBugdetSubcategoryAsync(subcategoryId));
        }

        [HttpGet("personal/transaction/{goalId}")]

        public Task<IActionResult> GetUserTransactionsGoalAsync(Guid goalId, [FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _financialGoalService.GetUserTransactionsGoalAsync(goalId, paginationParameter));
        }

        [HttpGet("personal/{goalId}")]

        public Task<IActionResult> GetPersonalFinancialGoalById(Guid goalId)
        {
            return ValidateAndExecute(() => _financialGoalService.GetPersonalFinancialGoalByIdAsync(goalId));
        }

        [HttpGet("personal/chart/{goalId}")]

        public Task<IActionResult> GetChartPersonalFinacialGoalByIdAsync(Guid goalId, string type)
        {
            return ValidateAndExecute(() => _financialGoalService.GetChartPersonalFinacialGoalByIdAsync(goalId, type));
        }

        [HttpGet("personal/user-spending-model/{id}")]

        public Task<IActionResult> GetPersonalFinancialGoalBySpendingModelAsync(Guid id, [FromQuery] PaginationParameter paginationParameter, [FromQuery] FinancialGoalFilter filter)
        {
            return ValidateAndExecute(() => _financialGoalService.GetUserFinancialGoalBySpendingModelAsync(id, paginationParameter, filter));
        }

        [HttpGet("personal/create/available-categories")]

        public Task<IActionResult> GetAvailableCategoriesCreateGoalUserAsync()
        {
            return ValidateAndExecute(() => _financialGoalService.GetAvailableCategoriesCreateGoalPersonalAsync());
        }

        [HttpPut("personal")]

        public Task<IActionResult> UpdatePersonalFinancialGoal([FromBody] UpdatePersonalFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.UpdatePersonalFinancialGoalAsync(model));
        }

        [HttpDelete("personal")]

        public Task<IActionResult> DeletePersonalFinancialGoal([FromBody] DeleteFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.DeletePersonalFinancialGoalAsync(model));
        }

        #endregion

        #region Group Financial Goals

        [HttpPost("group")]
        public Task<IActionResult> AddGroupFinancialGoal([FromBody] AddGroupFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.AddGroupFinancialGoalAsync(model));
        }

        [HttpGet("group")]
        public Task<IActionResult> GetGroupFinancialGoals([FromQuery] GetGroupFinancialGoalsModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.GetGroupFinancialGoalsAsync(model));
        }

        [HttpGet("group/detail")]
        public Task<IActionResult> GetGroupFinancialGoalById([FromQuery] GetGroupFinancialGoalDetailModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.GetGroupFinancialGoalByIdAsync(model));
        }

        [HttpPut("group")]
        public Task<IActionResult> UpdateGroupFinancialGoal([FromBody] UpdateGroupFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.UpdateGroupFinancialGoalAsync(model));
        }

        [HttpDelete("group/{id}")]
        public Task<IActionResult> DeleteGroupFinancialGoal(Guid id)
        {
            return ValidateAndExecute(() => _financialGoalService.DeleteGroupFinancialGoalAsync(new DeleteFinancialGoalModel { Id = id }));
        }

        [HttpPost("approve-group-goal")]
        public Task<IActionResult> ApproveGroupFinancialGoal([FromBody] ApproveGroupFinancialGoalRequestModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.ApproveGroupFinancialGoalAsync(model));
        }

        #endregion
    }
}
