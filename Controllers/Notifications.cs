using Job_Finder.Models;
using Job_Finder.Services;
using Job_Finder.Services.AutoApplyService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Job_Finder.Controllers
{
    public class Notifications : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SaveJobs _saveJobs;
        public Notifications(AppDbContext context, UserManager<AppUser> userManager, SaveJobs saveJobs)
        {
            _context = context;
            _userManager = userManager;
            _saveJobs = saveJobs;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var jobList = await _context.Jobs.ToListAsync();
            ViewBag.UserData = user.LastAdSeen;
            return View(jobList);
        }
        public async Task<IActionResult> Apply()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(await _context.UserNotifications.ToListAsync());
        }
        public async Task<IActionResult> View(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var job = await _context.Jobs.FindAsync(id);
            foreach (var jobs in _context.Jobs)
            {
                if (job.Data > jobs.Data && jobs.Data > user.LastAdSeen)
                {
                    DateTime aux = job.Data;
                    job.Data = jobs.Data;
                    jobs.Data = aux;
                }
            }
            user.LastAdSeen = job.Data;
            await _context.SaveChangesAsync();
            return View(job);
        }

        public async Task<IActionResult> SetAsSaved(int id)
        {
            var notification = await _context.UserNotifications.FindAsync(id);
            var job = await _context.Jobs.FindAsync(notification.JobId);
            await _saveJobs.SaveAsAppliedAsync(job);
            notification.ApplicationStatus = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Apply");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.UserNotifications.FindAsync(id);
            _context.UserNotifications.Remove(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction("Apply");
        }
        public async Task<IActionResult>Notification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var countJobs = await _context.Jobs
                .Where(n => n.Data > user.LastAdSeen)
                .CountAsync();
            var countNotification = await _context.UserNotifications.
                Where(n => n.UserId == user.Id && !n.ApplicationStatus).CountAsync(); 
            int count = countJobs + countNotification;
            return Json(new {count});
        }
        public async Task<IActionResult> NotificationJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var countJobs = await _context.Jobs
                .Where(n => n.Data > user.LastAdSeen)
                .CountAsync();
            return Json(new { count = countJobs });
        }

        public async Task<IActionResult> NotificationApply()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var countNotification = await _context.UserNotifications
                .Where(n => n.UserId == user.Id && !n.ApplicationStatus)
                .CountAsync();
            return Json(new { count = countNotification });
        }


        public async Task<IActionResult> SeeAll()
        {
            var user = await _userManager.GetUserAsync(User);
            DateTime aux = DateTime.MinValue;
            foreach (var job in _context.Jobs)
            {
                if (job.Data > aux)
                {
                    aux = job.Data;
                }
            }
            user.LastAdSeen = aux;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");  
        }
    }
}
