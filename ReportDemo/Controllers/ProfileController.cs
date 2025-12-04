using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;

namespace ReportDemo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (profile == null)
            {
                // Create a new profile if it doesn't exist
                profile = new UserProfile
                {
                    UserId = userId!
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId!
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile profile, IFormFile? profilePicture)
        {
            var userId = _userManager.GetUserId(User);
            
            if (userId != profile.UserId)
            {
                return Forbid();
            }

            try
            {
                // Get existing profile from database
                var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (existingProfile == null)
                {
                    // Create new profile if it doesn't exist
                    existingProfile = new UserProfile
                    {
                        UserId = userId!,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserProfiles.Add(existingProfile);
                }
                
                // Update profile fields
                existingProfile.FirstName = profile.FirstName?.Trim();
                existingProfile.LastName = profile.LastName?.Trim();
                existingProfile.PhoneNumber = profile.PhoneNumber?.Trim();
                existingProfile.Bio = profile.Bio?.Trim();
                existingProfile.UpdatedAt = DateTime.UtcNow;
                
                // Handle profile picture upload
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Only JPG, PNG, GIF, and WebP files are allowed.";
                        return View(profile);
                    }
                    
                    // Validate file size (max 5MB)
                    if (profilePicture.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size cannot exceed 5MB.";
                        return View(profile);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }
                    
                    // Verify file was saved
                    if (System.IO.File.Exists(filePath))
                    {
                        // Delete old profile picture if it exists
                        if (!string.IsNullOrEmpty(existingProfile.ProfilePicturePath))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, existingProfile.ProfilePicturePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        existingProfile.ProfilePicturePath = "/uploads/profiles/" + uniqueFileName;
                    }
                    else
                    {
                        TempData["Error"] = "Failed to save profile picture.";
                        return View(profile);
                    }
                }

                // Save changes
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error updating profile: {ex.Message}");
                TempData["Error"] = $"An error occurred while updating your profile: {ex.Message}";
                
                // Return the view with the model to show validation errors
                return View(profile);
            }
        }
    }
}