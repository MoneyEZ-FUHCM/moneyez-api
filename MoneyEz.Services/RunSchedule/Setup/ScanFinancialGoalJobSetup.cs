using Microsoft.Extensions.Options;
using MoneyEz.Services.RunSchedule.Job;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.RunSchedule.Setup
{
    public class ScanFinancialGoalJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobkey = JobKey.Create(nameof(ScanFinancialGoalJob));
            options.AddJob<ScanFinancialGoalJob>(JobBuilder => JobBuilder.WithIdentity(jobkey))
            .AddTrigger(trigger =>
                trigger.ForJob(jobkey).WithCronSchedule("0 0 0 * * ?",
                    x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")))); // chạy mỗi 0:00 mỗi ngày
                                                                                                       //trigger.ForJob(jobkey).WithCronSchedule("0 * * * * ?")); // chạy mỗi phút 1 lần
        }
    }
}
