using Microsoft.AspNetCore.Identity;

namespace Job_Finder.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public DateTime CreateAt { get; set; }

    }
}
