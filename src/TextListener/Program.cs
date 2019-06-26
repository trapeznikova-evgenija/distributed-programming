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
               string[] messageData = message.ToString().Split('/');
               if (messageData[0] == "TextCreated")
               {
                   IDatabase mainDb = redis.GetDatabase(0);
                   string dbIndex = mainDb.StringGet((string)messageData[1]);
                   IDatabase db = redis.GetDatabase(Convert.ToInt32(dbIndex));
                   string value = db.StringGet((string)messageData[1]);
                   Console.WriteLine($"{(string)messageData[1]} {value} database name: {dbIndex}");
               }
           });
            Console.ReadLine();
        }
    }
}
