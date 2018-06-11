using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Azure.WebJobs;

namespace ServiceBusMessageReceiver
{
    public class Functions
    {
        private const string QueueAnnounce = "testing";
        private static readonly Lazy<ILog> LazyLog = new Lazy<ILog>(() =>
        {
            var log = LogManager.GetLogger(typeof(Functions));
            XmlConfigurator.Configure();
            return log;
        });

        private static readonly ILog Logger = LazyLog.Value;
        public static void ProcessQueueMessage([ServiceBusTrigger(QueueAnnounce)] string queueMessage)
        {
            if (!string.IsNullOrEmpty(queueMessage))
            {
                Logger.Info($"Receive message at {DateTime.Now}: {queueMessage}.");
            }
        }
    }
}
