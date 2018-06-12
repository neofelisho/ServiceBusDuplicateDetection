using System;
using log4net;
using log4net.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusMessageReceiver
{
    public class Functions
    {
        private const string QueueName = "testing";

        private static readonly Lazy<ILog> LazyLog = new Lazy<ILog>(() =>
        {
            var log = LogManager.GetLogger(typeof(Functions));
            XmlConfigurator.Configure();
            return log;
        });

        private static readonly ILog Logger = LazyLog.Value;

        [Singleton("QueueLock", SingletonScope.Host)]
        public static void ProcessQueueMessage([ServiceBusTrigger(QueueName)] BrokeredMessage queueMessage)
        {
            try
            {
                //var myMessage = JsonConvert.DeserializeObject<MyMessage>(queueMessage.GetBody<string>());
                Logger.Info($"get body by MyMessage:{queueMessage.GetBody<string>()}");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }
    }

    public class MyMessage
    {
        public string Content { get; set; }
        public DateTime ScheduleTime { get; set; }

        public override string ToString()
        {
            return Content;
        }
    }
}