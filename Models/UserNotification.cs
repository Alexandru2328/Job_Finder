namespace Job_Finder.Models
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int JobId { get; set; }
        public bool ApplicationStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public Job Job { get; set; }
        public AppUser User { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public ICollection<AppUser> UserNotificationId { get; set; }
        public ICollection<Job> Jobs { get; set; }
    }
}
