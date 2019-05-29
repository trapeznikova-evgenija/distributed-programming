using System;
using StackExchange.Redis;

namespace TextListener
{
    class Program
    {
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        //сабскрайбер который получает события
        public static ISubscriber subscriber = tempDb.Multiplexer.GetSubscriber();

        static void Main(string[] args)
        {
            subscriber.Subscribe("events", (channel, message) => {
                string idStr = (string)message;
                string value = tempDb.StringGet(idStr);
                Console.WriteLine(idStr + ": " + value);
            });

            Console.ReadLine();
        }
    }
}
