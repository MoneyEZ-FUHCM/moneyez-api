using Microsoft.Extensions.Logging;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.Services.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.RunSchedule.Job
{
    public class ScanFinancialGoalJob : IJob
    {
        private readonly ILogger<ScanFinancialGoalJob> _logger;
        private readonly IUserSpendingModelService _userSpendingModelService;

        public ScanFinancialGoalJob(ILogger<ScanFinancialGoalJob> logger, IUserSpendingModelService userSpendingModelService)
        {
            _logger = logger;
            _userSpendingModelService = userSpendingModelService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("{Now} - Start - Running scan user spending model job", CommonUtils.GetCurrentTime());

                await _userSpendingModelService.ProcessExpiredAndUpcomingSpendingModelsAsync();

                _logger.LogInformation("{Now} - Done - Running scan user spending model job", CommonUtils.GetCurrentTime());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during running scan user spending model job.");
            }
            await Task.CompletedTask;
        }
    }
}
