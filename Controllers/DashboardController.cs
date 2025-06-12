using GloboClima.WebApp.Models;
using GloboClima.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.WebApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IWeatherService _weatherService;

        public DashboardController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public IActionResult Index()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            ViewBag.UserEmail = userEmail;
            return View();
        }
    }

}
