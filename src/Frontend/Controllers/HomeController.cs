using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TextDetails(string id)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync("http://127.0.0.1:5000/api/values/" + id);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            ViewData["TextDetails"] = responseBody;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data)
        {
            HttpClient restClient = new HttpClient();
            
            restClient.BaseAddress = new Uri("http://localhost:5000/");
            restClient.DefaultRequestHeaders.Clear();
           
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("value", data)
            });

            HttpResponseMessage response = await restClient.PostAsync("/api/values", content);
            string id = await response.Content.ReadAsStringAsync();

            return new RedirectResult("http://localhost:5001/Home/TextDetails/" + id);
        }

        public IActionResult Error()
        {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
