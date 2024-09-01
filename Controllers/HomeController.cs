using Job_Finder.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Job_Finder.Controllers
{
    public class HomeController : Controller
    {
        private readonly Automation _automation;
        public HomeController(Automation automation)
        {
            _automation = automation;
        }

        public async Task <IActionResult> Index()
        {
            await Task.Delay(1000);
            //await _automation.JobFinderProcess();
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
