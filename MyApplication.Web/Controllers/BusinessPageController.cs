using MyApplication.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using System.Collections.Generic;
using System.Linq;

using ModelsTask = MyApplication.Web.Models.Task;
using ModelsTaskStatus = MyApplication.Web.Models.TaskStatus;

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
            IEnumerable<ModelsTask> tasks = Enumerable.Empty<ModelsTask>();
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
        public IActionResult UpdateStatus(int id, ModelsTaskStatus status)
        {
            var task = _context.Tasks.Find(id);
            if (task == null) return NotFound();

            task.Status = status;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteTask(int taskId)
        {
            var task = _context.Tasks.Find(taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditDescription(int taskId, string description)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.Description = description;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateTask(string title, string description, ModelsTaskStatus status, int userId)
        {
            var newTask = new ModelsTask
            {
                Title = title,
                Description = description,
                Status = status,
                CreatedDate = DateTime.Now,
                UserId = userId
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
