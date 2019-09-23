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
        private static string _limitIsOver = null;

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
            _redisService.GetDataFromDb((int)_redisService.GetDbNumberById(id), id);
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]TextDto value)
        {
            var id = Guid.NewGuid().ToString();
            _redisService.SetDataToDb(0, new RedisData(id, _redisHelper.GetDbNumberByRegionKind(value.RegionKind).ToString()));
            _redisService.SetDataToDb((int)_redisHelper.GetDbNumberByRegionKind(value.RegionKind), new RedisData(id, value.Value));
            _redisService.SendMessage((int)_redisHelper.GetDbNumberByRegionKind(value.RegionKind), "events", new RedisData()
            {
                Id = id,
                Value = value.Value
            }, "TextCreated");
            return id;
        }

        [Route("text-rank")]
        [HttpPost]
        public string CalculateTextRank([FromBody]string id)
        {
            string rank = null;
            string limitStatus = _redisService.GetValueById(0, $"LimitIsExceeded");
            if (limitStatus != "false")
            {
                for (int i = 0; i < 10; i++)
                {
                    rank = _redisService.GetValueById((int)_redisService.GetDbNumberById(id), $"rank_{id}");
                    if (rank != null)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            else
            {
                return "limitIsOver";
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
                avgRank = _redisService.GetDataFromDb(0, "avgRankPrefix_"),
                declineRankQuantity = _redisService.GetDataFromDb(0, "declineRankQuantityPrefix_") ?? 0.ToString()
            };
        }
    }
}
