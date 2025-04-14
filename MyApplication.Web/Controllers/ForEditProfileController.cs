using Microsoft.AspNetCore.Mvc;
using MyApplication.Web.Data;
using MyApplication.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace MyApplication.Web.Controllers
{
    public class ForEditProfileController : Controller
    {
        private readonly AppDbContext _context;
        public ForEditProfileController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileModel model)
        {
            {
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user.Id != model.Id)
                    model.Id = user.Id;
            }
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.Id);
                if (user != null)
                {
                    if (model.UserName != null)
                    {
                        bool userNameExists = await _context.Users.AnyAsync(u => u.UserName == model.UserName);
                        if (!userNameExists)
                        {
                            user.UserName = model.UserName;
                            HttpContext.Session.SetString("UserName", model.UserName);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanımda.");
                        }
                    }
                    if (model.Email != null)
                    {
                        bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                        if (!emailExists)
                        {
                            user.Email = model.Email;
                            HttpContext.Session.SetString("Email", model.Email);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
                        }
                    }
                    if (model.Password != null)
                    {
                        user.Password = model.Password;
                        HttpContext.Session.SetString("Password", model.Password);
                    }
                    if (model.DateOfBirth != null)
                    {
                        if (model.DateOfBirth < DateTime.Now.Date)
                            user.DateOfBirth = model.DateOfBirth?.Date ?? DateTime.Today;
                        else
                            ModelState.AddModelError("", "İleri bir tarih eklenemez.");
                    }
                    if (model.File != null)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        //create folder if not exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        //get file extension
                        FileInfo fileInfo = new FileInfo(model.File.FileName);
                        string fileName = model.File.FileName;

                        string fileNameWithPath = Path.Combine(path, fileName);

                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            model.File.CopyTo(stream);
                        }

                        user.PhotoPath = model.File.FileName;
                    }
                    await _context.SaveChangesAsync();
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        TempData["Error"] = error.ErrorMessage; // Veya hataları dizi olarak saklayabilirsiniz.
                    }
                    return RedirectToAction("Profile", "Home");
                }
            }
            return View("Index");
        }

    }
}
