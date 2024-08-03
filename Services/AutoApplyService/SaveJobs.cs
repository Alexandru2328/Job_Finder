using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Job_Finder.Services.AutoApplyService
{
    public class SaveJobs
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveJobs(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task SaveAsAppliedAsync(Job jobSent)
        {
            var user = await GetCurrentUserAsync();

            var job = await _context.Jobs.FindAsync(jobSent.Id);
            if (job == null)
            {
                return;
            }

            DateTime aux = job.Data;
            job.Data = user.LastDataApply;
            user.LastDataApply = aux;

            _context.Jobs.Update(job);
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
            Console.WriteLine("A fost salvat ca aplicat.");
        }

        public async Task SaveAsNotAppliedAsync(Job jobSent)
        {
            var job = await _context.Jobs.FindAsync(jobSent.Id);
            if (job == null)
            {
                return;
            }

            job.Data = DateTime.Now;

            _context.Jobs.Update(job);

            await _context.SaveChangesAsync();
            Console.WriteLine("A fost salvat pentru mai târziu.");
        }
    }
}
