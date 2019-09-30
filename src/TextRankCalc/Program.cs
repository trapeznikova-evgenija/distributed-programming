using System;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        private static ConnectionMultiplexer redis;
        const string CHANNEL_NAME = "CalculateVowelConsJob";
        const string QUEUE_NAME = "vowel-cons-counter-jobs";
        static void Main( string[] args )
        {
            Console.WriteLine( "TextRank STARTED" );
            redis = ConnectionMultiplexer.Connect( "127.0.0.1:6379" );
            ISubscriber sub = redis.GetSubscriber();

            sub.Subscribe( "events", ( channel, message ) =>
            {
                string[] messageData = message.ToString().Split( '/' );
                if ( messageData[0] == "ProcessingAccepted" && messageData[2] == "true" )
                {
                    IDatabase mainDb = redis.GetDatabase( 0 );
                    string dbIndex = mainDb.StringGet( ( string ) messageData[ 1 ] );
                    IDatabase redisDb = redis.GetDatabase( Convert.ToInt32( dbIndex ) );
                    string value = redisDb.StringGet( ( string ) messageData[ 1 ] );
                    SendMessage( $"{messageData[1]}/{value}", redis.GetDatabase( 0 ) );
                    Console.WriteLine( $"{messageData[1]}: {value} db:{dbIndex}" );
                }
            });
            Console.ReadLine();
        }

        private static void SendMessage( string message, IDatabase redisDb )
        {
            // put message to queue
            redisDb.ListLeftPush( QUEUE_NAME, message, flags: CommandFlags.FireAndForget );
            // and notify consumers
            redisDb.Multiplexer.GetSubscriber().Publish( CHANNEL_NAME, "" );
        }
    }
}
