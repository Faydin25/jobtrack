using MyApplication.Web.Models; // Doğru namespace
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MyApplication.Web.Data;

namespace MyApplication.Web.Controllers
{
    public class BusinessPageController : Controller
    {
        private readonly AppDbContext _context;
        public BusinessPageController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? userId)
        {
            var users = _context.Users.ToList();
            IEnumerable<MyApplication.Web.Models.Task> tasks = Enumerable.Empty<MyApplication.Web.Models.Task>();
            if (userId.HasValue)
            {
                tasks = _context.Tasks
                                .Include(t => t.User)
                                .Where(t => t.UserId == userId.Value)
                                .ToList();
            }

            var viewModel = new BusinessPageViewModel
            {
                Users = users,
                Tasks = tasks
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeleteTask(int taskId)
        {
            var task = _context.Tasks.Find(taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
                TempData["Message"] = "Görev başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Görev bulunamadı.";
            }
            return RedirectToAction("Index"); // Görev listesi sayfasına geri yönlendir
        }

        [HttpPost]
        public IActionResult EditDescription(int taskId, string description)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.Description = description;
                _context.SaveChanges();
                TempData["Message"] = "Açıklama güncellendi.";
            }
            else
            {
                TempData["Error"] = "Görev bulunamadı.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateTask(string title, string description, Models.TaskStatus status, int userId)
        {
            var newTask = new Models.Task
            {
                Title = title,
                Description = description,
                Status = status,
                CreatedDate = DateTime.Now,
                UserId = userId // Kullanıcı ID'sini atayın
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();

            TempData["Message"] = "Yeni görev başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }
    }
}
