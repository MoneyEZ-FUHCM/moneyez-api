using Microsoft.Extensions.Options;
using MoneyEz.API.RunSchedule.Job;
using Quartz;


namespace MoneyEz.API.RunSchedule.Setup
{
    public class RecurringTransactionJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(RecurringTransactionJob));

            options.AddJob<RecurringTransactionJob>(jobBuilder =>
                jobBuilder.WithIdentity(jobKey))
                .AddTrigger(triggerBuilder =>
                    triggerBuilder
                        .ForJob(jobKey)
                        .WithCronSchedule("0 0 0 * * ?", x => x
                            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))));
        }
    }
}
