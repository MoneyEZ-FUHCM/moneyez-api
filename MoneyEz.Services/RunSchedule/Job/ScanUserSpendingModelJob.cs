using Microsoft.Extensions.Logging;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using Quartz;

namespace MoneyEz.Services.RunSchedule.Job
{
    public class ScanUserSpendingModelJob : IJob
    {
        private readonly ILogger<ScanUserSpendingModelJob> _logger;
        private readonly IFinancialGoalService _financialGoalService;

        public ScanUserSpendingModelJob(ILogger<ScanUserSpendingModelJob> logger, IFinancialGoalService financialGoalService)
        {
            _logger = logger;
            _financialGoalService = financialGoalService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("{Now} - Start - Running scan financial goal job", CommonUtils.GetCurrentTime());

                await _financialGoalService.ScanAndChangeStatusWithDueGoalAsync();

                _logger.LogInformation("{Now} - Done - Running scan financial goal job", CommonUtils.GetCurrentTime());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during running scan financial goal job.");
            }
            await Task.CompletedTask;
        }
    }
}
