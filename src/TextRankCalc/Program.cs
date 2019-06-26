using System;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        private static ConnectionMultiplexer redis;
        const string CHANNEL_NAME = "CalculateVowelConsJob";
        const string QUEUE_NAME = "vowel-cons-counter-jobs";
        static void Main(string[] args)
        {
            redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            ISubscriber sub = redis.GetSubscriber();

            sub.Subscribe("events", (channel, message) =>
            {
                IDatabase mainDb = redis.GetDatabase(0);
                string dbIndex = mainDb.StringGet((string)message);
                IDatabase redisDb = redis.GetDatabase(Convert.ToInt32(dbIndex));
                string value = redisDb.StringGet((string)message);
                SendMessage($"{message}/{value}", redis.GetDatabase(0));
                Console.WriteLine($"{message}: {value} database number: {dbIndex}");
            });
            Console.ReadLine();
        }

        private static void SendMessage(string message, IDatabase redisDb)
        {
            // put message to queue
            redisDb.ListLeftPush(QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            // and notify consumers
            redisDb.Multiplexer.GetSubscriber().Publish(CHANNEL_NAME, "");
        }
    }
}
