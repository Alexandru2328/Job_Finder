using Job_Finder.Models;
using Job_Finder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace WebAppScraping.Controllers
{
    [Authorize]
    public class WebScrapingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly WebScrapingService _webScrapingService;
        private readonly UserManager<AppUser> _userManager;

        public WebScrapingController(AppDbContext context, WebScrapingService webScrapingService, UserManager<AppUser> userManager)
        {
            _context = context;
            _webScrapingService = webScrapingService;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var jobList = await _context.Jobs.OrderByDescending(j => j.Data)
                    .ToListAsync();
                ViewBag.LastDataApplied = user.LastDataApply;
                ViewBag.Count = jobList.Count;
                return View(jobList);
            }
            return RedirectToAction("Identity", "Login" );
        }

        public async Task<IActionResult> Search()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.LastDataSearch = DateTime.Now;
                await _context.SaveChangesAsync();
                await _webScrapingService.SearchJobs();

                return RedirectToAction("ShowResult");
            }
            return RedirectToAction("Identity", "Login");
        }
        public async Task<IActionResult> ShowResult()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var lastSearch = user.LastDataSearch;
                var allJobs = await _context.Jobs.ToListAsync();
                var filteredJobs = allJobs.Where(j => j.Data >= lastSearch).ToList();
                ViewBag.TotalCount = filteredJobs.Count;
                ViewBag.LastDataApplied = user.LastDataApply;
                return View(filteredJobs);
            }
            return RedirectToAction("Identity", "Login");
        }
    }
}
