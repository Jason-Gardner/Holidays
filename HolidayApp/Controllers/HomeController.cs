﻿using System;
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
        List<Holiday> holidayList = new List<Holiday>();
        public Holiday newDay = new Holiday();

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            if (holidayList.Count() == 0)
            {
                holidayList = await BuildList();
            }

            return View();
        }

        public IActionResult DateCheck()
        {
            Holiday tempDay = new Holiday();

            foreach (Holiday holiday in holidayList)
            {
                if (holiday.month == DateTime.Now.Month)
                {
                    if (holiday.day == DateTime.Now.Day)
                    {
                        tempDay = holiday;
                    }
                }
            }

            if (tempDay.name == null)
            {
                ViewBag.No = "There are no holidays today.";
            }

            return View("Index", tempDay);
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
                            newDay.name = list[i].GetProperty("name").ToString();
                            newDay.day = list[i].GetProperty("date").GetProperty("datetime").GetProperty("day").GetInt32();
                            newDay.month = list[i].GetProperty("date").GetProperty("datetime").GetProperty("month").GetInt32();

                            holidayList.Add(newDay);
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
