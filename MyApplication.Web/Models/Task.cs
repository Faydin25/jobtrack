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
        public int UserId { get; set; }  // Foreign key User modelin Id alanına bağlı

        public string? Description { get; set; }
        public string? Title { get; set; }

        public TaskStatus Status { get; set; }  // Enum kullanarak task durumunu yönet

        public virtual User User { get; set; }  // Navigation property

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }  // Oluşturulma tarihi
    }

    public enum TaskStatus
    {
        ToDo = 0,  // Yapılacak
        InProgress = 1,  // Yapılıyor
        Done = 2 // Tamamlandı
    }
}
