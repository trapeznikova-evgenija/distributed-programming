using System;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        public static ISubscriber subscriber = tempDb.Multiplexer.GetSubscriber();

        static void Main(string[] args)
        {
            subscriber.Subscribe("events", (channel, message) => {
                string value = tempDb.StringGet((string)message);
                int vowelsAmount = Regex.Matches(value, @"[aeiouy]", RegexOptions.IgnoreCase).Count;
                int consonantsAmount = Regex.Matches(value, @"[bcdfghjklmnpqrstvwxz]", RegexOptions.IgnoreCase).Count;

                double rank = (double)vowelsAmount / consonantsAmount;
                string id = "rank_" + (string)message;
                tempDb.StringSet(id, rank);
              
                Console.WriteLine("id: " + id);
                Console.WriteLine("vowelsAmount:" + vowelsAmount);
                Console.WriteLine("consonantsAmount:" + consonantsAmount);
                Console.WriteLine("rank: " + tempDb.StringGet(id));
            });

            Console.ReadLine(); 
        }
    }
}
