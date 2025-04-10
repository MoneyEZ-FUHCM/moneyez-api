using Microsoft.Extensions.Logging;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Utils;
using Quartz;

namespace MoneyEz.Services.RunSchedule.Job
{
    public class RecurringTransactionJob : IJob
    {
        private readonly ILogger<RecurringTransactionJob> _logger;
        private readonly IRecurringTransactionService _recurringTransactionService;

        public RecurringTransactionJob(
            ILogger<RecurringTransactionJob> logger,
            IRecurringTransactionService recurringTransactionService)
        {
            _logger = logger;
            _recurringTransactionService = recurringTransactionService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("{Now} - Start RecurringTransactionJob", CommonUtils.GetCurrentTime());

                await _recurringTransactionService.GenerateTransactionsFromRecurringAsync();

                _logger.LogInformation("{Now} - Done RecurringTransactionJob", CommonUtils.GetCurrentTime());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RecurringTransactionJob failed");
            }

            await Task.CompletedTask;
        }
    }
}
