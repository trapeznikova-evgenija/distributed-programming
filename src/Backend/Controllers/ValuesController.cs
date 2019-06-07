using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Configuration;
using System.Threading;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();
        public static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        public static IDatabase tempDb = redis.GetDatabase();
        public static ISubscriber subscriber = redis.GetSubscriber();

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _data.TryGetValue(id, out value);

            string currentId = (string)id;
            var valueInfo = "";
            valueInfo = tempDb.StringGet("rank_" + currentId);
            
            if (valueInfo == "")
            {
                for (int i = 0; i < 3; i++)
                {
                    valueInfo = tempDb.StringGet("rank_" + currentId);
                    
                    if (valueInfo == "")
                    {
                        Thread.Sleep(300);
                    } else
                    {
                        break;
                    }
                }    
            }

            Console.WriteLine("rank: " + valueInfo);
            Console.WriteLine("rankId: " + "rank_" + currentId);

            return valueInfo;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromForm]string value)
        {
            var id = Guid.NewGuid().ToString();
            _data[id] = value;
            tempDb.StringSet(id, value);

            subscriber.Publish("events", id);

            return id;
        }
    }
}
