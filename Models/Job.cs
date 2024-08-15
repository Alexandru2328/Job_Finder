using System.ComponentModel.DataAnnotations;

namespace Job_Finder.Models
{
    public class Job

    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Id_Traking { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Details { get; set; }
        public string Company { get; set; }
        public DateTime Data { get; set; }
        public string Platform { get; set; }
        public ICollection<UserNotification> UserNotifications { get; set; }

    }
}
