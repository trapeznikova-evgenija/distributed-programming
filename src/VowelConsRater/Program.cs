using System;
using StackExchange.Redis;

namespace VowelConsRater
{
    class Program
    {
        const string VOWEL_CONS_COUNTER_CHANNEL = "RateVowelConsJob";
        const string VOWEL_CONS_COUNTER_QUEUE_NAME = "vowel-cons-rater-jobs";

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsRater was started");
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe(VOWEL_CONS_COUNTER_CHANNEL, delegate
                {
                    IDatabase redisDb = redis.GetDatabase(0);
                    string message = redisDb.ListRightPop(VOWEL_CONS_COUNTER_QUEUE_NAME);
                    while (message != null)
                    {
                        string[] messageData = message.Split('/');
                        string id = messageData[0];
                        double vowels = Convert.ToDouble(messageData[1]);
                        double consonants = Convert.ToDouble(messageData[2]);
                        double result = vowels / consonants;
                        string rankId = "rank_" + id;
                        string dbIndex = redisDb.StringGet(id);
                        IDatabase bd = redis.GetDatabase(Convert.ToInt32(dbIndex));
                        bd.StringSet(rankId, result);
                        Console.WriteLine($"{rankId} : {result} db:{dbIndex}");
                        message = redisDb.ListRightPop(VOWEL_CONS_COUNTER_QUEUE_NAME);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
