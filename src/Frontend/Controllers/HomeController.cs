using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using System.IO;
using Frontend.Services;
using Frontend.Kinds;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        ConnectionService _connectionService;

        public HomeController()
        {
            _connectionService = new ConnectionService("http://127.0.0.1:5000/");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data, RegionKind regionKind)
        {
            if (!String.IsNullOrEmpty(data))
            {
                string id = null;
                //TODO: send data in POST request to backend and read returned id value from response
                id = await _connectionService.PostRequestToBackend(new TextDto(data, regionKind), "api/values");
                return RedirectToAction("GetTextDetails", new { id = id });
            }
            else
            {
                return View("Error", "Ошибка ввода данных");
            }
        }

        [Route("text-details")]
        [HttpGet]
        public async Task<IActionResult> GetTextDetails(string id)
        {
            string rank = await _connectionService.PostRequestToBackend(id, $"api/values/text-rank");

            if (!String.IsNullOrEmpty(rank))
            {
                return View("TextDetails", rank);
            }

            return View("Error", rank ?? "Ошибка ввода данных");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
