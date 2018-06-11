using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using StackExchange.Redis;

namespace ServiceBusDuplicateMessageSender
{
    public class Functions
    {
        private const string QueueAnnounce = "testing";
        private const string KeyAnnounce = "job:testing";

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
            return new QueueClient(ServiceBusConnectionString, QueueAnnounce);
        });

        private static readonly QueueClient QueueClient = LazyQueueClient.Value;

        public static async Task EnqueueMessageToAzureServiceBus([TimerTrigger("0 0 * * * *", RunOnStartup = true)]
            TimerInfo myTimer)
        {
            var currentMinute = new DateTime(2018, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            for (var i = 1; i <= 12; i++)
            {
                var scheduleTime = currentMinute.AddMinutes(i * 10);
                if (scheduleTime < DateTime.Now) continue;

                var scheduleUtc = new DateTimeOffset(scheduleTime.ToUniversalTime());
                var message = new Message(Encoding.UTF8.GetBytes($"Scheduled at {scheduleTime}."))
                {
                    MessageId = scheduleUtc.ToUnixTimeSeconds().ToString()
                };
                var redisKey = $"{KeyAnnounce}:{message.MessageId}";
                var oldValue = RedisDb.StringGet(redisKey);
                try
                {
                    if (!oldValue.IsNullOrEmpty)
                    {
                        Logger.Info($"Cancel scheduled message: {message}:{oldValue}.");
                        await QueueClient.CancelScheduledMessageAsync(long.Parse(oldValue.ToString()));
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Cancel scheduled message failed: {message}.");
                    Logger.Error(e.Message);
                    continue;
                }

                try
                {
                    var sequenceNumber = await QueueClient.ScheduleMessageAsync(message, scheduleUtc);
                    var newValue = $"{sequenceNumber}";
                    Logger.Info(oldValue.IsNullOrEmpty
                        ? $"Enqueue at {scheduleTime}: {message}."
                        : $"Update schedule to {scheduleTime}: {message}.");
                    await RedisDb.StringSetAsync(redisKey, newValue, scheduleTime.Subtract(DateTime.Now));
                }
                catch (Exception e)
                {
                    Logger.Error($"Enqueue scheduled message failed: {message}");
                    Logger.Error(e.Message);
                }
            }
        }
    }
}