using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Job_Finder.Services.AutoApplyService
{
    public class SaveJobs
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NotificationServices _notification;

        public SaveJobs(AppDbContext context, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, NotificationServices notification)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _notification = notification;
        }

        public async Task<AppUser> GetCurrentUserAsync()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return await _userManager.FindByIdAsync(userId);

            } catch (Exception ex) { }
            return null;
        }

        public async Task SaveAsAppliedAsync(Job jobSent)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobSent.Id);
                var user = await GetCurrentUserAsync();

                if (job == null)
                {
                    return;
                }

                DateTime aux =job.Data;
                foreach (var jobInList in _context.Jobs)
                {
                    if (aux > jobInList.Data && jobInList.Data > user.LastDataApply)
                    {
                        aux = jobInList.Data;
                        jobInList.Data = job.Data;
                        job.Data = aux;
                    }
                }
                user.LastDataApply = aux;

                await _context.SaveChangesAsync();
                Console.WriteLine("A fost salvat ca aplicat.");

            } catch (Exception ex) { }
        }
        
        public async Task SaveAsNotAppliedAsync(Job jobSent)
        {
            var user = await GetCurrentUserAsync();
            var job = await _context.Jobs.FindAsync(jobSent.Id);
            if (job == null)
            {
                return;
            }

            job.Data = DateTime.Now;

            _context.Jobs.Update(job);
            await _notification.CreateNotification(user, job);
            await _context.SaveChangesAsync();
            Console.WriteLine("A fost salvat pentru mai târziu.");
        }
    }
}
