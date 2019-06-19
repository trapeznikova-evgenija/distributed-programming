using System;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        const string QUEUE_NAME = "vowel-cons-counter-jobs";
        const string TASK_NAME = "CalculateVowelConsJob";

        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        public static ISubscriber subscriber = tempDb.Multiplexer.GetSubscriber();

        static void Main(string[] args)
        {
            subscriber.Subscribe("events", (channel, message) => {

                string value = tempDb.StringGet((string)message);
                SendMessage((string)message, tempDb);
              
                Console.WriteLine((string)message);
            });

            Console.ReadLine(); 
        }
         private static void SendMessage(string message, IDatabase tempDb)
        {
            tempDb.ListLeftPush(QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            tempDb.Multiplexer.GetSubscriber().Publish(TASK_NAME, "");
        }
    }
}
