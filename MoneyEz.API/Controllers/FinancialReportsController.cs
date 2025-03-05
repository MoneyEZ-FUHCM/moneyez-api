using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/financial-reports")]
    [ApiController]
    [Authorize]
    public class FinancialReportsController : BaseController
    {
        private readonly IFinancialReportService _financialReportService;

        public FinancialReportsController(IFinancialReportService financialReportService)
        {
            _financialReportService = financialReportService;
        }

        // --- User Reports ---

        [HttpGet("user")]
        public Task<IActionResult> GetAllUserReports([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _financialReportService.GetAllReportsForUserAsync(paginationParameter));
        }

        [HttpPost("user")]
        public Task<IActionResult> CreateUserReport([FromBody] CreateUserReportModel model)
        {
            return ValidateAndExecute(() => _financialReportService.CreateUserReportAsync(model));
        }

        [HttpPut("user")]
        public Task<IActionResult> UpdateUserReport([FromBody] UpdateUserReportModel model)
        {
            return ValidateAndExecute(() => _financialReportService.UpdateUserReportAsync(model));
        }

        [HttpDelete("user/{reportId}")]
        public Task<IActionResult> DeleteUserReport(Guid reportId)
        {
            return ValidateAndExecute(() => _financialReportService.DeleteUserReportAsync(reportId));
        }

        [HttpGet("user/{reportId}")]
        public Task<IActionResult> GetUserReportById(Guid reportId)
        {
            return ValidateAndExecute(() => _financialReportService.GetUserReportByIdAsync(reportId));
        }

        // --- Group Reports ---

        [HttpGet("group")]
        public Task<IActionResult> GetAllGroupReports([FromQuery] PaginationParameter paginationParameter, [FromQuery] Guid groupId)
        {
            return ValidateAndExecute(() => _financialReportService.GetAllReportsForGroupAsync(paginationParameter, groupId));
        }

        [HttpPost("group")]
        public Task<IActionResult> CreateGroupReport([FromBody] CreateGroupReportModel model)
        {
            return ValidateAndExecute(() => _financialReportService.CreateGroupReportAsync(model));
        }

        [HttpPut("group")]
        public Task<IActionResult> UpdateGroupReport([FromBody] UpdateGroupReportModel model)
        {
            return ValidateAndExecute(() => _financialReportService.UpdateGroupReportAsync(model));
        }

        [HttpDelete("group/{reportId}")]
        public Task<IActionResult> DeleteGroupReport(Guid reportId)
        {
            return ValidateAndExecute(() => _financialReportService.DeleteGroupReportAsync(reportId));
        }

        [HttpGet("group/{reportId}")]
        public Task<IActionResult> GetGroupReportById(Guid reportId)
        {
            return ValidateAndExecute(() => _financialReportService.GetGroupReportByIdAsync(reportId));
        }
    }
}
