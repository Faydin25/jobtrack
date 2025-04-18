using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace MyApplication.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için AntiForgery token ekleniyor
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();  // Çıkış işlemi
            return RedirectToAction("Index", "Home");  // Ana sayfaya yönlendirme
        }
    }
}