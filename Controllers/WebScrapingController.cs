using Job_Finder.Models;
using Job_Finder.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace WebAppScraping.Controllers
{
    public class WebScrapingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WebScrapingService _webScrapingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WebScrapingController(ApplicationDbContext context, WebScrapingService webScrapingService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webScrapingService = webScrapingService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var jobList = await _context.Jobs.ToListAsync();
            ViewBag.LastDataApplied = user.LastDataApply;
            return View(jobList);
        }

        public async Task<IActionResult> Search()
        {
            var user = await _userManager.GetUserAsync(User);
            var lastSearch = user.LastDataSearch;
            user.LastDataSearch = DateTime.Now;
            await _webScrapingService.SearchJobs();
           
            return RedirectToAction("ShowResult");
        }



        public async Task<IActionResult> ShowResult()
        {
            var user = await _userManager.GetUserAsync(User);
            var lastSearch = user.LastDataSearch;
            var allJobs = await _context.Jobs.ToListAsync();
            var filteredJobs = allJobs.Where(j => j.Data >= lastSearch).ToList();
            ViewBag.TotalCount = filteredJobs.Count;
            ViewBag.LastDataApplied = user.LastDataApply;
            return View(filteredJobs);
        }

   


    }
}
