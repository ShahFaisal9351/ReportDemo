using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;

namespace ReportDemo.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserAvatarViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Content(""); // Return empty if not authenticated
            }

            var userId = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            var viewModel = new UserAvatarViewModel
            {
                Username = User.Identity?.Name ?? "User",
                ProfilePicturePath = profile?.ProfilePicturePath,
                FirstName = profile?.FirstName,
                LastName = profile?.LastName
            };

            return View(viewModel);
        }
    }

    public class UserAvatarViewModel
    {
        public string Username { get; set; } = "";
        public string? ProfilePicturePath { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        public string DisplayName => !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName) 
            ? $"{FirstName} {LastName}".Trim() 
            : Username;
            
        public string InitialLetter => !string.IsNullOrEmpty(FirstName) 
            ? FirstName.Substring(0, 1).ToUpper()
            : Username.Substring(0, 1).ToUpper();
    }
}