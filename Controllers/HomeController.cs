using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Covid19.Models;
using Covid19.Services;

namespace Covid19.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICovidService _covidService;

        public HomeController(ICovidService covidService)
        {
            _covidService = covidService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Countries = await _covidService.GetCovidAsync();
            return View();
        }

        public async Task<IActionResult> CountryDaywiseDetails(string country)
        {
            ViewBag.Country = country;
            ViewBag.CountryData = await _covidService.GetDayWiseCountryDetailsAsync(country);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
