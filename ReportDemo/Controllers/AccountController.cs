using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReportDemo.Models;
using System.Threading.Tasks;

namespace ReportDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Sign in non-persistent (auto-logout on browser close)
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home"); // redirect to dashboard
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home"); // redirect to dashboard
            }

            // More detailed error messages for debugging
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked out.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Account not allowed to sign in.");
            }
            else if (result.RequiresTwoFactor)
            {
                ModelState.AddModelError(string.Empty, "Two-factor authentication required.");
            }
            else
            {
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "No account found with this email address.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid password.");
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Clear all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            return RedirectToAction("Login", "Account");
        }
        
        // GET: Account/ClearSession - for troubleshooting
        public async Task<IActionResult> ClearSession()
        {
            await _signInManager.SignOutAsync();
            
            // Clear all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            HttpContext.Session.Clear();
            
            TempData["Message"] = "All authentication data cleared. Please login again.";
            return RedirectToAction("Login", "Account");
        }
        
        // GET: Account/UserInfo - for debugging (only in development)
        public async Task<IActionResult> UserInfo()
        {
            if (!_userManager.Users.Any())
            {
                ViewBag.Message = "No users found in database. Please register first.";
                return View();
            }
            
            var users = _userManager.Users.Take(10).ToList();
            ViewBag.Users = users;
            ViewBag.Message = $"Found {_userManager.Users.Count()} users in database:";
            
            return View();
        }
        
        // GET: Account/ForceLogout - clear all authentication (development only)
        public async Task<IActionResult> ForceLogout()
        {
            // Sign out from Identity
            await _signInManager.SignOutAsync();
            
            // Clear ALL cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie, new CookieOptions { Path = "/" });
                Response.Cookies.Delete(cookie, new CookieOptions { Path = "/", Domain = Request.Host.Host });
            }
            
            // Clear session if exists
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Clear();
            }
            
            TempData["Message"] = "All authentication cleared! Please login again.";
            return RedirectToAction("Login", "Account");
        }
    }
}
