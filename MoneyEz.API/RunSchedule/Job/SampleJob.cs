using Quartz;

namespace MoneyEz.API.RunSchedule.Job
{
    public class SampleJob : IJob
    {
        private readonly ILogger<SampleJob> _logger;

        public SampleJob(ILogger<SampleJob> logger)
        {
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("{Now} - Start - Running sample job", DateTime.Now);

                await Task.Delay(5000); // Sleep for 5 seconds

                _logger.LogInformation("{Now} - Done - Running sample job", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during running sample job.");
            }
            await Task.CompletedTask;
        }
    }
}
