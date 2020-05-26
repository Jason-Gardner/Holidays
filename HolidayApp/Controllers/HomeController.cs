using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HolidayApp.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace HolidayApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _config;
        private JsonDocument jDoc;
        public List<Holiday> holidayList = new List<Holiday>();
        public Holiday newDay = new Holiday();

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DateCheck()
        {
            ViewBag.Today = null;
            holidayList = await BuildList();
            List<Holiday> tempList = new List<Holiday>();

            foreach (Holiday holiday in holidayList)
            {
                if (holiday.month == DateTime.Now.Month && holiday.day == DateTime.Now.Day)
                {
                    ViewBag.Today = ($"Today is {holiday.name}!");
                }
            }

            if (tempList.Count == 0)
            {
                ViewBag.Today = "There are no holidays today.";
            }

            return View("Index", tempList);
        }

        public async Task<IActionResult> NextCheck()
        {
            holidayList = await BuildList();
            ViewBag.Today = null;

            List<Holiday> tempList = new List<Holiday>();

            foreach (Holiday holiday in holidayList)
            {
                if (holiday.month == DateTime.Now.Month)
                {
                    if (holiday.day > DateTime.Now.Day && tempList.Count == 0)
                    {
                        tempList.Add(holiday);
                    }
                }
            }

            return View("Index", tempList);
        }

        public async Task<IActionResult> MonthCheck()
        {
            holidayList = await BuildList();
            ViewBag.Today = null;

            List<Holiday> tempList = new List<Holiday>();

            foreach (Holiday holiday in holidayList)
            {
                if (holiday.month == DateTime.Now.Month && holiday.day > DateTime.Now.Day)
                {
                    tempList.Add(holiday);
                }
            }

            var month = DateTime.Now.Month;
            string word = "";

            List<string> currentMonth = new List<string>()
            {
                "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
            };

            word = currentMonth[month - 1];

            ViewBag.Month = ($"The remaining holidays for the month of {word} are:");

            return View("Index", tempList);
        }

        public async Task<List<Holiday>> BuildList()
        {
            if (holidayList.Count() == 0)
            {
                var key = _config["Key"];

                var holiday = ($"https://calendarific.com/api/v2/holidays?&api_key={key}&country=US&year=2020");

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(holiday))
                    {
                        var summary = await response.Content.ReadAsStringAsync();
                        jDoc = JsonDocument.Parse(summary);

                        var list = jDoc.RootElement.GetProperty("response").GetProperty("holidays");

                        for (int i = 0; i < list.GetArrayLength(); i++)
                        {
                            holidayList.Add(new Holiday()
                            {
                                name = list[i].GetProperty("name").ToString(),
                                day = list[i].GetProperty("date").GetProperty("datetime").GetProperty("day").GetInt32(),
                                month = list[i].GetProperty("date").GetProperty("datetime").GetProperty("month").GetInt32()
                            }
                            );
                        }

                    }
                }
            }

            return holidayList;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
