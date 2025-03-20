using MoneyEz.Repositories.Utils;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using Quartz;

namespace MoneyEz.API.RunSchedule.Job
{
    public class ScanUserSpendingModelJob : IJob
    {
        private readonly ILogger<ScanUserSpendingModelJob> _logger;
        private readonly IUserSpendingModelService _userSpendingModelService;

        public ScanUserSpendingModelJob(ILogger<ScanUserSpendingModelJob> logger, IUserSpendingModelService userSpendingModelService)
        {
            _logger = logger;
            _userSpendingModelService = userSpendingModelService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("{Now} - Start - Running scan user spending model job", CommonUtils.GetCurrentTime());

                await _userSpendingModelService.UpdateExpiredSpendingModelsAsync();

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
