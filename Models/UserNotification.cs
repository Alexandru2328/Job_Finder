namespace Job_Finder.Models
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int JobId { get; set; }
        public bool ApplicationStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }
}
