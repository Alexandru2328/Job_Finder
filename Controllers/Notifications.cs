using Job_Finder.Models;
using Job_Finder.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Job_Finder.Controllers
{
    public class Notifications : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public Notifications(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var jobList = await _context.Jobs.ToListAsync();
            ViewBag.UserData = user.LastAdSeen;
            return View(jobList);
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
        public async Task <IActionResult> Notification() 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var count = await _context.Jobs
                .Where(n => n.Data > user.LastAdSeen)
                .CountAsync();

            return Json(new { count });
        }
    }
}
