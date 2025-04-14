using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyApplication.Web.Controllers
{
    public class MainPageController : Controller
    {
        private readonly AppDbContext _context;
        public MainPageController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            string? _UserName = HttpContext.Session.GetString("UserName");
            var _TUser = _context.Users.FirstOrDefault(u => u.UserName == _UserName);
            var _UserLogs = _TUser.LogTimesJson;

            var users = _context.Users.Include(u => u.Tasks).ToList();

            var _AgeGroups = GroupsAge(users);

            var _UserCount = _context.Users.Count();

            var _TasksAndCount = GetCountTasks(users);


            //var _TaskData = _context.Tasks.ToList();

            //ViewData["UserData"] = _UserData;
            //ViewData["TaskData"] = _TaskData;

            var _usersWithPhoto = users.Count(u => !string.IsNullOrEmpty(u.PhotoPath));
            var _usersWithoutPhoto = users.Count(u => string.IsNullOrEmpty(u.PhotoPath));

            ViewData["UsersWithPhoto"] = _usersWithPhoto;
            ViewData["UsersWithoutPhoto"] = _usersWithoutPhoto;

            ViewData["AgeGroups"] = _AgeGroups;
            ViewData["UserCount"] = _UserCount;
            ViewData["TasksAndCount"] = _TasksAndCount;
            ViewData["UserLogs"] = _UserLogs;
            return View();
        }

        public int CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
            {
                return 0;
            }
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;

            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }

        private Dictionary<int, int> GroupsAge(List <User> users)
        {
            var ageGruops = new Dictionary<int, int>();
            foreach (var user in users)
            {
                var age = CalculateAge(user.DateOfBirth);
                if (ageGruops.ContainsKey(age)) ageGruops[age]++;
                else
                    ageGruops[age] = 1;
            }
            return ageGruops;
        }

        private Dictionary<string, int> GetCountTasks(List<User> users)
        {
            var countTasks = new Dictionary<string, int>
    {
        { "ToDo", 0 },
        { "InProgress", 0 },
        { "Done", 0 }
    };

            foreach (var user in users)
            {
                foreach (var task in user.Tasks)
                {
                    switch (task.Status)
                    {
                        case Models.TaskStatus.ToDo:
                            countTasks["ToDo"]++;
                            break;
                        case Models.TaskStatus.InProgress:
                            countTasks["InProgress"]++;
                            break;
                        case Models.TaskStatus.Done:
                            countTasks["Done"]++;
                            break;
                    }
                }
            }

            return countTasks;
        }
    }
}
