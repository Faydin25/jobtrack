using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using MyApplication.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyApplication.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        //------------------------------------------------

        private readonly AppDbContext _context;
        private readonly EmailValidatorService _emailValidatorService;
        public HomeController(AppDbContext context, EmailValidatorService emailValidatorService)
        {
            _context = context;
            _emailValidatorService = emailValidatorService;
        }
        //------------------------------------------------

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            // Kullanıcı adı veya e-posta sistemde var mı?
            bool userNameExists = await _context.Users.AnyAsync(u => u.UserName == model.UserName);
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);

            if (userNameExists || emailExists)
            {
                if (userNameExists)
                {
                    ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanımda.");
                }
                if (emailExists)
                {
                    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
                }
                return View();
            }

            // Email domain geçerliliği kontrolü
            if (!_emailValidatorService.CheckEmailDomainExists(model.Email))
            {
                ModelState.AddModelError("Email", "Geçersiz e-posta domaini.");
                return View();
            }

            // Kayıt işlemi
            var user = new User { UserName = model.UserName, Email = model.Email, Password = model.Password };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Profile()
        {
            var model = new User();
            var userName = HttpContext.Session.GetString("UserName");
            //var Id = HttpContext.Session.GetInt32("Id");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            model = user ?? new User();

            return View(model);
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

            // JSON'dan log kayıtlarını deserialize edin
            var logs = JsonConvert.DeserializeObject<List<DateTime>>(user.LogTimesJson) ?? new List<DateTime>();
            logs = logs.OrderByDescending(log => log).ToList();

            return View(logs);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties 
                    { 
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    });

                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetInt32("UserId", user.Id);

                // Update login logs
                var logTimes = JsonConvert.DeserializeObject<List<DateTime>>(user.LogTimesJson ?? "[]");
                logTimes.Add(DateTime.Now);
                if (logTimes.Count > 10)
                {
                    logTimes = logTimes.OrderByDescending(l => l).Take(10).ToList();
                }
                user.LogTimesJson = JsonConvert.SerializeObject(logTimes);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "MainPage");
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult ToggleTheme()
        {
            var currentTheme = Request.Cookies["theme"];
            var newTheme = currentTheme == "dark" ? "light" : "dark";
            Response.Cookies.Append("theme", newTheme, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Delete all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            // Add cache control headers to prevent browser caching
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            // Redirect to login page
            return RedirectToAction("Index", "Home", null, "https");
        }

    }
}
