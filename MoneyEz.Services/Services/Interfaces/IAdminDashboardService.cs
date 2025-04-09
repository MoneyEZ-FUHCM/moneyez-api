using System;
using System.Threading.Tasks;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<BaseResultModel> GetDashboardStatisticsAsync();

        Task<BaseResultModel> GetModelUsageStatisticsAsync();
    }
}
