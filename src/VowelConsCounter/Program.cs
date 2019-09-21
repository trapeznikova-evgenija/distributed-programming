using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace VowelConsCounter
{
    class Program
    {

        const string TEXT_RANK_CHANNEL_NAME = "CalculateVowelConsJob";
        const string TEXT_RANK_QUEUE_NAME = "vowel-cons-counter-jobs";
        const string VOWEL_CONS_COUNTER_CHANNEL = "RateVowelConsJob";
        const string VOWEL_CONS_COUNTER_QUEUE_NAME = "vowel-cons-rater-jobs";
        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsCounter was started");
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
                ISubscriber sub = redis.GetSubscriber();
                TextRank textRank = new TextRank();
                sub.Subscribe(TEXT_RANK_CHANNEL_NAME, delegate
                {
                    IDatabase redisDb = redis.GetDatabase(0);
                    string message = redisDb.ListRightPop(TEXT_RANK_QUEUE_NAME);
                    while (message != null)
                    {
                        string[] messageData = message.Split('/');
                        string id = messageData[0];
                        string value = messageData[1];
                        int vowels = textRank.GetVowelsAmount(value);
                        int consonants = textRank.GetConsonantAmount(value);

                        SendMessage($"{id}/{vowels}/{consonants}", redisDb);
                        Console.WriteLine($"{id}: {vowels} {consonants}");
                        message = redisDb.ListRightPop(TEXT_RANK_QUEUE_NAME);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        private static void SendMessage(string message, IDatabase db)
        {
            // put message to queue
            db.ListLeftPush(VOWEL_CONS_COUNTER_QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            // and notify consumers
            db.Multiplexer.GetSubscriber().Publish(VOWEL_CONS_COUNTER_CHANNEL, "");
        }
    }

}
