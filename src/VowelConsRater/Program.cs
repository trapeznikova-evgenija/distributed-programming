using System;
using StackExchange.Redis;
using System.Collections.Generic;

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
               string message = tempDb.ListRightPop(RATE_QUEUE_NAME);
               
               while (message != null)
               {
                    string[] messageSplitArray = message.Split('/');

                    string contextId = messageSplitArray[0];
                    int vowelsAmount = Convert.ToInt16(messageSplitArray[1]);
                    int consonantsAmount = Convert.ToInt16(messageSplitArray[2]);

                    double rank = (double)vowelsAmount / consonantsAmount;
                    string id = "rank_" + (string)contextId;
                    tempDb.StringSet(id, rank);
                    var dataValue = tempDb.StringGet(id);
                    Console.WriteLine($"{dataValue}");

                    Console.WriteLine($"{id} : {rank}");

                    message = tempDb.ListRightPop(RATE_QUEUE_NAME);
                    subscriber.Publish("events", $"{id} : {rank}");
               }
           });

            Console.ReadLine(); 
        }       
    }
}
