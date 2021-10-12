using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatBot
{
    public class JobScheduler
    {
        private static StdSchedulerFactory _factory;
        private static IScheduler _scheduler;

        public static async Task SetupJobScheduler()
        {
            //construct the scheduler factory
            _factory = new StdSchedulerFactory();
            _scheduler = await _factory.GetScheduler();
            await _scheduler.Start();

            var sendTweetTrigger = TriggerBuilder.Create().WithIdentity("sendTweetTrigger").WithCronSchedule("0 0/20 * 1/1 * ? *").Build();
            var sendTweet = JobBuilder.Create<SyncSpreadsheetJob>().WithIdentity("sendTweet").Build();

            await _scheduler.ScheduleJob(sendTweet, sendTweetTrigger);
            Log.Information("[Job Scheduler] Job Scheduler Setup");
        }
    }

    internal class SyncSpreadsheetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var rnd = new Random();
            var imgName = Directory.GetFiles("catPics").Length;

            await Twitter.SendTweet(rnd.Next(0, imgName));
            Log.Information("[Twitter Job Schedule] Job processed");
        }
    }
}
