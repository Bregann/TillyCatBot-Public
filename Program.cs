using Serilog;
using System;
using System.Threading.Tasks;

namespace CatBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Setup logging
            Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.File("Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day)).WriteTo.Console().CreateLogger();

            Twitter.SetupTwitter();
            await JobScheduler.SetupJobScheduler();
            RedditData.SetupReddit();
            
            await Task.Delay(-1);
        }
    }
}
