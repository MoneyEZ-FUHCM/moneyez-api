using Microsoft.Extensions.Options;
using MoneyEz.API.RunSchedule.Job;
using Quartz;

namespace MoneyEz.API.RunSchedule.Setup
{
    public class ScanUserSpendingModelJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobkey = JobKey.Create(nameof(ScanUserSpendingModelJob));
            options.AddJob<ScanUserSpendingModelJob>(JobBuilder => JobBuilder.WithIdentity(jobkey))
            .AddTrigger(trigger =>
                trigger.ForJob(jobkey).WithCronSchedule("0 0 0 * * ?")); // chạy mỗi 0:00 mỗi ngày
                //trigger.ForJob(jobkey).WithCronSchedule("0 * * * * ?")); // chạy mỗi phút 1 lần
        }
    }
}
