using Microsoft.AspNetCore.Mvc;

namespace Job_Finder.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
