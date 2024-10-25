using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Job_Finder.Models;
using Job_Finder.Services;
using OpenQA.Selenium.Support.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace Job_Finder.Controllers
{
    [Authorize]
    public class AutoApplyController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly Automation _service;
        private readonly AppDbContext _context;
        private readonly AutomationProgress _progress;
        private int Percentage { get; set; } = 0;
        public AutoApplyController(UserManager<AppUser> userManager, Automation service, AppDbContext context, AutomationProgress progress)
        {
            _userManager = userManager;
            _service = service;
            _context = context;
            _progress = progress;
        }

        [HttpGet]
        public  Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string UserPlatformEmail, string UserPlatformPassword,
        int DomainExperience, string Message, string KeywordList, string MailCompani)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.UserPlatformEmail = UserPlatformEmail;
                user.UserPlatformPassword = UserPlatformPassword;
                user.DomainExperience = DomainExperience;
                user.Message = Message;
                user.KeywordList = KeywordList;
                user.MailCompany = MailCompani;
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
                await _service.JobFinderProcess();
            try
            {

            } catch (Exception ex) { }
        }

        public async Task<IActionResult>Apply(int id)
        {
            await _service.JobFinderProces(id);
            //var jobs = await _context.Jobs.ToListAsync();  // Selectezi toate joburile

            //if (jobs.Any())  // Verifici dacă există joburi
            //{
            //    _context.Jobs.RemoveRange(jobs);  // Ștergi toate joburile dintr-o singură operație
            //    await _context.SaveChangesAsync();  // Salvezi modificările o singură dată
            //}


            return RedirectToAction("View", "AutoApply");
        }
        public async Task<IActionResult> GetPercentage()
        {
            Percentage = await _progress.GetProgress();
            Console.WriteLine(Percentage);
            return Json(new { count = Percentage });
        }
    }
}
