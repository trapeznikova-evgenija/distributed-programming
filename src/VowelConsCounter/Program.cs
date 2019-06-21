using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VowelConsCounter
{
   class Program
    {
        const string VOWEL_COUNTER_JOBS_QUEUE = "vowel-cons-counter-jobs";
        const string VOWEL_RATER_JOBS_QUEUE = "vowel-cons-rater-jobs";
        const string CALCULATE_VOWEL_JOB_TASK_NAME = "CalculateVowelConsJob";
        const string RATE_VOWEL_CONS_JOB_TASK_NAME = "RateVowelConsJob";
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        public static ISubscriber subscriber = tempDb.Multiplexer.GetSubscriber();

        static void Main(string[] args)
        {
           subscriber.Subscribe(CALCULATE_VOWEL_JOB_TASK_NAME, delegate
           {
               string contextId = tempDb.ListRightPop(VOWEL_COUNTER_JOBS_QUEUE);

               while (contextId != null)
                {
                    string value = tempDb.StringGet(contextId);

                    int vowelsAmount = Regex.Matches(value, @"[aeiouy]", RegexOptions.IgnoreCase).Count;
                    int consonantsAmount = Regex.Matches(value, @"[bcdfghjklmnpqrstvwxz]", RegexOptions.IgnoreCase).Count;
               
                    SendMessage(tempDb, $"{contextId}/{vowelsAmount}/{consonantsAmount}");
                    Console.WriteLine("Send message with RateVowelConsJob" + " " + contextId + " " + vowelsAmount + " " + consonantsAmount);

                    contextId = tempDb.ListRightPop(VOWEL_COUNTER_JOBS_QUEUE);
                }
            });

           Console.ReadLine(); 
        }
         private static void SendMessage(IDatabase tempDb, string vowelNumConsNum)
        {
            tempDb.ListLeftPush(VOWEL_RATER_JOBS_QUEUE, vowelNumConsNum, flags: CommandFlags.FireAndForget);
            tempDb.Multiplexer.GetSubscriber().Publish(RATE_VOWEL_CONS_JOB_TASK_NAME, "");
        }
    }
}
