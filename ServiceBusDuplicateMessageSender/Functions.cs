using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ServiceBusDuplicateMessageSender
{
    public class Functions
    {
        private const string QueueName = "testing";
        private const string RedisKeyPrefix = "job:testing";

        private static readonly string RedisConnectionString =
            ConfigurationManager.ConnectionStrings["RedisConnection"]?.ConnectionString;

        private static readonly Lazy<ILog> LazyLog = new Lazy<ILog>(() =>
        {
            var log = LogManager.GetLogger(typeof(Functions));
            XmlConfigurator.Configure();
            return log;
        });

        private static readonly ILog Logger = LazyLog.Value;

        private static readonly Lazy<IConnectionMultiplexer> LazyRedis = new Lazy<IConnectionMultiplexer>(() =>
        {
            if (string.IsNullOrEmpty(RedisConnectionString))
                throw new Exception("Empty Redis Connection!");
            return ConnectionMultiplexer.Connect(RedisConnectionString);
        });

        private static readonly Lazy<IDatabase> LazyRedisDb = new Lazy<IDatabase>(() => LazyRedis.Value.GetDatabase());
        private static readonly IDatabase RedisDb = LazyRedisDb.Value;

        private static readonly string ServiceBusConnectionString =
            AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);

        private static readonly Lazy<QueueClient> LazyQueueClient = new Lazy<QueueClient>(() =>
        {
            if (string.IsNullOrEmpty(ServiceBusConnectionString))
                throw new Exception("Service bus connection string is missing.");
            return QueueClient.CreateFromConnectionString(ServiceBusConnectionString, QueueName);
        });

        private static readonly QueueClient QueueClient = LazyQueueClient.Value;

        public static void EnqueueMessageToAzureServiceBus([TimerTrigger("0 0 * * * *", RunOnStartup = true)]
            TimerInfo myTimer)
        {
            var currentMinute = new DateTime(2018, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            Enumerable.Range(1, 60).AsParallel().ForAll(async i => { await Genereate(currentMinute, i); });

        }

        private static async Task Genereate(DateTime baseTime, int i)
        {
            var scheduleTime = baseTime.AddMinutes(i);
            if (scheduleTime < DateTime.Now) return;

            var scheduleUtc = new DateTimeOffset(scheduleTime.ToUniversalTime());

            //var message = new BrokeredMessage(JsonConvert.SerializeObject(myMessage))
            //{
            //    MessageId = scheduleUtc.ToUnixTimeSeconds().ToString(),
            //    ContentType = "application/json"
            //};
            var message = new BrokeredMessage($"Scheduled at {scheduleTime}.")
            {
                MessageId = scheduleUtc.ToUnixTimeSeconds().ToString(),
            };
            var redisKey = $"{RedisKeyPrefix}:{message.MessageId}";
            var oldValue = RedisDb.StringGet(redisKey);
            try
            {
                if (!oldValue.IsNullOrEmpty)
                {
                    await QueueClient.CancelScheduledMessageAsync(long.Parse(oldValue.ToString()));
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Cancel scheduled message failed: {message}.");
                Logger.Error(e.Message);
                return;
            }

            try
            {
                var sequenceNumber = await QueueClient.ScheduleMessageAsync(message, scheduleUtc);
                var newValue = $"{sequenceNumber}";
                await RedisDb.StringSetAsync(redisKey, newValue, scheduleTime.Subtract(DateTime.Now));
            }
            catch (Exception e)
            {
                Logger.Error($"Enqueue scheduled message failed: {message}");
                Logger.Error(e.Message);
            }
        }
    }


    public class MyMessage
    {
        public string Content { get; set; }
        public DateTime ScheduleTime { get; set; }
    }
}