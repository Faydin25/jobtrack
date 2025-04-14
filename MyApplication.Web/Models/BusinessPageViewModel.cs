// BusinessPageViewModel.cs
namespace MyApplication.Web.Models
{
    public class BusinessPageViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<MyApplication.Web.Models.Task> Tasks { get; set; }
    }
}
