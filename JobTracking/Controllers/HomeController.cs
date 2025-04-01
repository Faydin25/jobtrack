using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace MyApplication.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TaskList()
        {
            var viewModel = new BusinessPageViewModel
            {
                Tasks = _context.Tasks.Include(t => t.User).ToList()
            };
            return View(viewModel);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            bool userNameExists = await _context.Users.AnyAsync(u => u.UserName == model.UserName);
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);

            if (userNameExists || emailExists)
            {
                if (userNameExists)
                {
                    ModelState.AddModelError("", "Bu kullanýcý adý zaten kullanýmda.");
                }
                if (emailExists)
                {
                    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanýmda.");
                }
                return View();
            }

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                IPAddress = GetIpAddress() // IP adresini kaydet
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetInt32("UserId", user.Id);

                // IP adresini güncelle
                user.IPAddress = GetIpAddress();

                // JSON'dan log kayýtlarýný deserialize edin
                var logTimes = JsonConvert.DeserializeObject<List<DateTime>>(user.LogTimesJson) ?? new List<DateTime>();
                logTimes.Add(DateTime.Now);

                if (logTimes.Count > 10)
                {
                    logTimes = logTimes.OrderByDescending(l => l).Take(10).ToList();
                }

                user.LogTimesJson = JsonConvert.SerializeObject(logTimes);
                _context.SaveChanges();

                return RedirectToAction("Profile");
            }
            else
            {
                ModelState.AddModelError("", "Kullanýcý adý veya þifre hatalý.");
                return View("Index");
            }
        }

        public async Task<IActionResult> Profile()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return View(user ?? new User());
        }

        public IActionResult UserLogs()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var logs = JsonConvert.DeserializeObject<List<DateTime>>(user.LogTimesJson) ?? new List<DateTime>();
            logs = logs.OrderByDescending(log => log).ToList();

            return View(logs);
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        private string GetIpAddress()
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            return remoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
