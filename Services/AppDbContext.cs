using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Job_Finder.Services
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<UserCookie> UserCookies { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";

            var client = new IdentityRole("client");
            admin.NormalizedName = "client";
            builder.Entity<IdentityRole>().HasData(admin, client);

            builder.Entity<UserNotification>()
                .HasOne(un => un.User).WithMany(u => u.UserNotifications)
                .HasForeignKey(un => un.UserId);

            builder.Entity<UserNotification>()
                .HasOne(un => un.Job).WithMany(j => j.UserNotifications)
                .HasForeignKey(un => un.JobId);
        }
    }
}
