using System.ComponentModel.DataAnnotations;

namespace MyApplication.Web.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? Password { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhotoPath { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
        public string LogTimesJson { get; set; } = "[]";
    }
}
