using Microsoft.Extensions.Options;
using MoneyEz.API.RunSchedule.Job;
using Quartz;

namespace MoneyEz.API.RunSchedule.Setup
{
    public class SampleJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobkey = JobKey.Create(nameof(SampleJob));
            options.AddJob<SampleJob>(JobBuilder => JobBuilder.WithIdentity(jobkey))
            .AddTrigger(trigger =>
                //trigger.ForJob(jobkey).WithCronSchedule("0 * * * * ?")); // chạy mỗi phút 1 lần
                trigger.ForJob(jobkey).WithCronSchedule("0 0 0 * * ?")); // chạy mỗi 0:00 mỗi ngày
        }
    }
}
