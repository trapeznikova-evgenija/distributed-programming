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
using Backend.Helpers;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private RedisService _redisService;
        private RedisHelper _redisHelper;

        public ValuesController(ConnectionMultiplexer redis)
        {
            _redisService = new RedisService(redis);
            _redisHelper = new RedisHelper();
        }

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _redisService.GetDataFromDb((int) _redisService.GetDbNumberById(id), id);
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]TextDto value)
        {
            var id = Guid.NewGuid().ToString();
            _redisService.SetDataToDb(0, new RedisData(id, _redisHelper.GetDbNumberByRegionKind(value.RegionKind).ToString()));
            _redisService.SetDataToDb((int) _redisHelper.GetDbNumberByRegionKind(value.RegionKind), new RedisData(id, value.Value));
            _redisService.SendMessage((int) _redisHelper.GetDbNumberByRegionKind(value.RegionKind), "events", new RedisData()
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
            for (int i = 0; i < 5; i++)
            {
                rank = _redisService.GetValueById((int) _redisService.GetDbNumberById(id), $"rank_{id}");
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

        [Route("statistics")]
        [HttpGet]
        public StatisitcDto GetStatistics()
        {
            return new StatisitcDto()
            {
                textNum = _redisService.GetDataFromDb(0, "textNum_"),
                highRankPart = _redisService.GetDataFromDb(0, "highRankPart_"),
                avgRank = _redisService.GetDataFromDb(0, "avgRankPrefix_")
            };
        }
    }
}
