using System;

namespace VowelConsRater
{
    class Program
    {
        const string RATE_QUEUE_NAME = "vowel-cons-rater-jobs";
        const string RATE_TASK_NAME = "RateVowelConsJob";

        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        public static ISubscriber subscriber = tempDb.Multiplexer.GetSubscriber();

        static void Main(string[] args)
        {
           subscriber.Subscribe(RATE_TASK_NAME, delegate
           {
               
           });
        }
    }
}
