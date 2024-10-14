using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Job_Finder.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public DateTime LastAdSeen { get; set; }
        public DateTime CreateAt { get; set; }
        public bool AutoAplly { get; set; } = false;
        public DateTime LastDataSearch { get; set; }
        public DateTime LastDataApply { get; set; }
        public bool VipUser { get; set; } = false;
        public string UserPlatformEmail { get; set; } = string.Empty;
        public string UserPlatformPassword { get; set; } = string.Empty;
        public int DomainExperience { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
        public string MailCompani { get; set; } = string.Empty;
        public string KeywordList { get; set; } = string.Empty;
        public ICollection<UserNotification> UserNotifications { get; set; }
        public string UserEmailKey { get; set; }

    }
}
