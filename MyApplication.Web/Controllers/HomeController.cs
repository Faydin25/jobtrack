﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using MyApplication.Web.Services;

namespace MyApplication.Web.Controllers
{


    public class HomeController : Controller
    {

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

        //------------------------------------------------

        private readonly AppDbContext _context;
        private readonly EmailValidatorService _emailValidatorService;
        public HomeController(AppDbContext context, EmailValidatorService emailValidatorService)
        {
            _context = context;
            _emailValidatorService = emailValidatorService;
        }
        //------------------------------------------------

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


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetInt32("UserId", user.Id);

                // JSON'dan log kayıtlarını deserialize edin
                var logTimes = JsonConvert.DeserializeObject<List<DateTime>>(user.LogTimesJson) ?? new List<DateTime>();

                // Yeni log kaydını ekleyin
                logTimes.Add(DateTime.Now);

                // Sadece son 10 log kaydını tutun
                if (logTimes.Count > 10)
                {
                    logTimes = logTimes.OrderByDescending(l => l).Take(10).ToList();
                }

                // Log kayıtlarını JSON olarak serialize edin ve kaydedin
                user.LogTimesJson = JsonConvert.SerializeObject(logTimes);
                _context.SaveChanges();

                return RedirectToAction("Index", "MainPage");
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View("Index");
            }
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


        public IActionResult Logout() //"Profile sayfasına koyucam.
        {
            return RedirectToAction("Index");
        }


    }
}
