using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SampleWebApp.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        TableData _tableData;
        public HomeController(TableData tableData)
        {
            _tableData = tableData;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users =await _tableData.GetUserInfo();
            return View(users);
        }

        [HttpGet]
        public IActionResult Activate(DurableResponse response)
        {
            return View("Activate", response.Id);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(string instanceId)
        {
            var httpClient = new HttpClient();
            if(string.IsNullOrEmpty(instanceId))
            {
                return View(instanceId);
            }
            var response = await httpClient.PostAsJsonAsync<string>("http://localhost:7071/api/DurableExternalEvent", instanceId);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(instanceId);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Common.User user)
        {
            var httpClient = new HttpClient();
            var response= await httpClient.PostAsJsonAsync<User>("http://localhost:7071/api/DurableStart", user);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var durableResponse = JsonConvert.DeserializeObject<DurableResponse>(responseBody);

                return RedirectToAction("Activate", durableResponse);
            }
            else
            {
                return View(user);
            }
        }
    }
}
