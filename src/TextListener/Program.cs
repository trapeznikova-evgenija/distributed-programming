using System;
using StackExchange.Redis;

namespace TextListener
{
    class Program
    {
        private static ConnectionMultiplexer redis;
        static void Main(string[] args)
        {
            redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            ISubscriber sub = redis.GetSubscriber();
            //IDatabase db = redis.GetDatabase();

            sub.Subscribe("events", (channel, message) =>
           {
               IDatabase mainDb = redis.GetDatabase(0);
               string dbIndex = mainDb.StringGet((string)message);
               IDatabase db = redis.GetDatabase(Convert.ToInt32(dbIndex));
               string value = db.StringGet((string)message);
               Console.WriteLine($"{(string)message} {value} db:{dbIndex}");
           });
            Console.ReadLine();
        }
    }
}
