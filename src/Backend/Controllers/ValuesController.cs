using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using Backend.Services;
using Backend.Models;
using System.Net.Http;
using System.Threading;
using Backend.Kinds;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private RedisService _redisService;

        public ValuesController(ConnectionMultiplexer redis)
        {
            _redisService = new RedisService(redis);
        }

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _redisService.GetDataFromDb((int) GetDbNumberById(id), id);
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]TextDto value)
        {
            var id = Guid.NewGuid().ToString();
            _redisService.SetDataToDb(0, new RedisData(id, GetDbNumberByRegionKind(value.RegionKind).ToString()));
            _redisService.SetDataToDb((int)GetDbNumberByRegionKind(value.RegionKind), new RedisData(id, value.Value));
            _redisService.SendMessage((int)GetDbNumberByRegionKind(value.RegionKind), "events", new RedisData()
            {
                Id = id,
                Value = value.Value
            });
            return id;
        }

        [Route("text-rank")]
        [HttpPost]
        public string CalculateTextRank([FromBody]string id)
        {
            string rank = null;
            for (int i = 0; i < 10; i++)
            {
                rank = _redisService.GetValueById((int) GetDbNumberById(id), $"rank_{id}");
                if (rank != null)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            return rank;
        }

        private int? GetDbNumberByRegionKind(RegionKind regionKind)
        {
            switch (regionKind)
            {
                case RegionKind.Ru:
                {
                    return 1;
                }
                case RegionKind.Eu:
                {
                    return 2;
                }
                case RegionKind.Usa:
                {
                    return 3;
                }
                default:
                {
                    return null;
                }
            }
        }

        private int GetDbNumberById(string id)
        {
            return Convert.ToInt32(_redisService.GetDataFromDb(0, id));
        }
    }
}
