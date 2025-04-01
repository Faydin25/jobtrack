using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApplication.Web.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }  // Foreign key User modelin Id alanýna baðlý

        public string? Description { get; set; }
        public string? Title { get; set; }

        public TaskStatus Status { get; set; }  // Enum kullanarak task durumunu yönet

        public virtual User User { get; set; }  // Navigation property

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }  // Oluþturulma tarihi
    }

    public enum TaskStatus
    {
        ToDo = 0,  // Yapýlacak
        InProgress = 1,  // Yapýlýyor
        Done = 2 // Tamamlandý
    }
}