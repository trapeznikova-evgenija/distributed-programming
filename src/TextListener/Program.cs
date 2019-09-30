﻿using System;
using StackExchange.Redis;

namespace TextListener
{
    class Program
    {
        private static ConnectionMultiplexer redis;
        static void Main(string[] args)
        {
            Console.WriteLine("TextListener STARTED");
            redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            ISubscriber sub = redis.GetSubscriber();
            //IDatabase db = redis.GetDatabase();
            sub.Subscribe("events", (channel, message) =>
           {
               string[] messageData = message.ToString().Split('/');
               
               if (messageData[0] == "TextCreated")
               {
                   Console.WriteLine(messageData[0]);
                   IDatabase mainDb = redis.GetDatabase(0);
                   string dbIndex = mainDb.StringGet((string)messageData[1]);
                   IDatabase db = redis.GetDatabase(Convert.ToInt32(dbIndex));
                   string value = db.StringGet((string)messageData[1]);
                
                   Console.WriteLine($"{messageData[1]} {value} db:{dbIndex}");
               }
           });
            Console.ReadLine();
        }
    }
}
