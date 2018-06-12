using System.Net;
using Microsoft.Azure.WebJobs;

namespace ServiceBusDuplicateMessageSender
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            var config = new JobHostConfiguration();

            if (config.IsDevelopment) config.UseDevelopmentSettings();

            config.UseTimers();
            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}