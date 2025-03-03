using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
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

        public Task<IActionResult> GetPersonalFinancialGoals([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _financialGoalService.GetPersonalFinancialGoalsAsync(paginationParameter));
        }

        [HttpPost("personal/detail")]

        public Task<IActionResult> GetPersonalFinancialGoalById([FromBody] GetPersonalFinancialGoalDetailModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.GetPersonalFinancialGoalByIdAsync(model));
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

        [HttpPost("group/detail")]
        public Task<IActionResult> GetGroupFinancialGoalById([FromBody] GetGroupFinancialGoalDetailModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.GetGroupFinancialGoalByIdAsync(model));
        }

        [HttpPut("group")]
        public Task<IActionResult> UpdateGroupFinancialGoal([FromBody] UpdateGroupFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.UpdateGroupFinancialGoalAsync(model));
        }

        [HttpDelete("group")]
        public Task<IActionResult> DeleteGroupFinancialGoal([FromBody] DeleteFinancialGoalModel model)
        {
            return ValidateAndExecute(() => _financialGoalService.DeleteGroupFinancialGoalAsync(model));
        }

        #endregion
    }
}
