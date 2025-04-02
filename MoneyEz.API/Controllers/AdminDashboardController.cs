using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.Services.Interfaces;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminDashboardController : BaseController
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("statistics")]
        public Task<IActionResult> GetDashboardStatistics()
        {
            return ValidateAndExecute(() => _adminDashboardService.GetDashboardStatisticsAsync());
        }

        [HttpGet("model-usage")]
        public Task<IActionResult> GetModelUsageStatistics()
        {
            return ValidateAndExecute(() => _adminDashboardService.GetModelUsageStatisticsAsync());
        }
    }
}
