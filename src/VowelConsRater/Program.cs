﻿using System;
using StackExchange.Redis;
using Redis;

namespace VowelConsRater
{
    class Program
    {
        const string VOWEL_CONS_COUNTER_CHANNEL = "RateVowelConsJob";
        const string VOWEL_CONS_COUNTER_QUEUE_NAME = "vowel-cons-rater-jobs";
        const string TEXT_RANK_CALCULATED_CHANNEL = "TextRankCalculated ";

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsRater STARTED");
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect( "127.0.0.1:6379" );
              //  RedisService redisService = new RedisService( "127.0.0.1:6379" );
                MessageParser messageParser = new MessageParser();
                ISubscriber sub = redis.GetSubscriber();
                sub.Subscribe( VOWEL_CONS_COUNTER_CHANNEL, delegate
                {
                    IDatabase redisDb = redis.GetDatabase( 0 );
                    string message = redisDb.ListRightPop( VOWEL_CONS_COUNTER_QUEUE_NAME );
                    while ( message != null )
                    {
                        messageParser.Parse( message );
                        double result = ( double ) ( messageParser.Vowels / messageParser.Consonants );
                        string rankId = "rank_" + messageParser.ContextId;
                        string dbIndex = redisDb.StringGet( messageParser.ContextId );
                        IDatabase db = redis.GetDatabase( Convert.ToInt32( dbIndex ) );
                        db.StringSet( rankId, result );
                        PublishEvent( $"TextRankCalculated/{result.ToString()}", redisDb, "events" );
                        Console.WriteLine($"{ rankId } : { result } db:{ dbIndex }");
                        message = redisDb.ListRightPop( VOWEL_CONS_COUNTER_QUEUE_NAME );
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        private static void PublishEvent(string message, IDatabase db, string channel)
        {
            // and notify consumers
            db.Multiplexer.GetSubscriber().Publish(channel, message);
        }
    }
}
