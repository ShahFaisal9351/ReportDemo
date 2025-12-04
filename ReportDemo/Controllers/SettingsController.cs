using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;

namespace ReportDemo.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            
            var settingsViewModel = new SettingsViewModel
            {
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                FirstName = "",
                LastName = "",
                Bio = "",
                NotificationsEnabled = true,
                EmailNotifications = true,
                Theme = "light",
                Language = "en",
                DateFormat = "MM/dd/yyyy",
                TimeFormat = "12h"
            };

            // Load user profile if exists
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                settingsViewModel.FirstName = profile.FirstName ?? "";
                settingsViewModel.LastName = profile.LastName ?? "";
                settingsViewModel.Bio = profile.Bio ?? "";
                settingsViewModel.ProfilePicturePath = profile.ProfilePicturePath ?? "";
            }

            return View(settingsViewModel);
        }

        // POST: Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var user = await _userManager.FindByIdAsync(userId);
                    
                    if (user != null)
                    {
                        // Update user email
                        if (user.Email != model.Email)
                        {
                            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                            if (!setEmailResult.Succeeded)
                            {
                                TempData["Error"] = "Failed to update email address.";
                                return View(model);
                            }
                        }

                        // Update user phone
                        user.PhoneNumber = model.PhoneNumber;
                        await _userManager.UpdateAsync(user);
                    }

                    // Update user profile
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                    if (profile == null)
                    {
                        profile = new UserProfile
                        {
                            UserId = userId!,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.UserProfiles.Add(profile);
                    }

                    profile.FirstName = model.FirstName?.Trim();
                    profile.LastName = model.LastName?.Trim();
                    profile.Bio = model.Bio?.Trim();
                    profile.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred: {ex.Message}";
                }
            }

            return View(model);
        }

        // POST: Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Password changed successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                    
                    if (profile == null)
                    {
                        profile = new UserProfile
                        {
                            UserId = userId!,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.UserProfiles.Add(profile);
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Only JPG, PNG, GIF, and WebP files are allowed.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    if (profilePicture.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size cannot exceed 5MB.";
                        return RedirectToAction(nameof(Index));
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }
                    
                    // Delete old profile picture if it exists
                    if (!string.IsNullOrEmpty(profile.ProfilePicturePath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, profile.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    profile.ProfilePicturePath = "/uploads/profiles/" + uniqueFileName;
                    profile.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Profile picture updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error uploading profile picture: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Please select a profile picture.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
