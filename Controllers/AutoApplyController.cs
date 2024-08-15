using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Job_Finder.Models;
using Job_Finder.Services;
using OpenQA.Selenium.Support.UI;
using Microsoft.EntityFrameworkCore;

namespace Job_Finder.Controllers
{
    [Authorize]
    public class AutoApplyController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly Automation _service;
        private readonly AppDbContext _context;
        public AutoApplyController(UserManager<AppUser> userManager, Automation service, AppDbContext context)
        {
            _userManager = userManager;
            _service = service;
            _context = context;
        }

        [HttpGet]
        public  Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string UserPlatformEmail, string UserPlatformPassword,
        int DomainExperience, string Message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.UserPlatformEmail = UserPlatformEmail;
                user.UserPlatformPassword = UserPlatformPassword;
                user.DomainExperience = DomainExperience;
                user.Message = Message;
            }          
            await _userManager.UpdateAsync(user);
            return RedirectToAction("View", "AutoApply");
        }

        public async Task <IActionResult> View()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
        public async Task AutoApplySession()
        {
            await _service.JobFinderProces();
        }

        public async Task Apply(int id)
        {
            //await _context.Database.ExecuteSqlRawAsync("DELETE FROM Jobs");
            await _service.JobFinderProces(id);
        }
    }
}
