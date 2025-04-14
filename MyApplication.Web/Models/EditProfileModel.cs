using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyApplication.Web.Models
{
    public class EditProfileModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email {  get; set; }
        public string? Password { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? File { get; set; }

    }
}
