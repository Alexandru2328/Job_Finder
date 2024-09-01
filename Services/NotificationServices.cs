using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Job_Finder.Services
{
    public class NotificationServices
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public NotificationServices(UserManager<AppUser> userManager, AppDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }
        private async Task<bool> IsSavedInDb(AppUser user, Job job)
        {
            var list = await _context.UserNotifications.Where(n => n.UserId == user.Id).
                ToListAsync();
            foreach (var notification in list)
            {
                if (notification.JobId == job.Id)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task CreateNotification(AppUser user, Job job)
        {
            var notification = new UserNotification
            {
                CreatedAt = DateTime.Now,
                ApplicationStatus = false,
                UserId = user.Id,
                JobId = job.Id,
                Link = job.Link,
                Title = job.Title,
            };
            bool exists = await IsSavedInDb(user, job);
            if (!exists)
            {
                _context.UserNotifications.Add(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
