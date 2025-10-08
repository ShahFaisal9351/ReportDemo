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

            if (ModelState.IsValid)
            {
                try
                {
                    // Debug: Log form data
                    Console.WriteLine($"Profile ID: {profile.Id}");
                    Console.WriteLine($"User ID: {userId}");
                    Console.WriteLine($"Profile Picture: {profilePicture?.FileName ?? "None"}");
                    
                    // Get existing profile from database
                    var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                    if (existingProfile == null)
                    {
                        TempData["Error"] = "Profile not found. Please try again.";
                        return View(profile);
                    }
                    
                    // Update profile fields
                    existingProfile.FirstName = profile.FirstName;
                    existingProfile.LastName = profile.LastName;
                    existingProfile.PhoneNumber = profile.PhoneNumber;
                    existingProfile.Bio = profile.Bio;
                    existingProfile.UpdatedAt = DateTime.UtcNow;
                    
                    // Handle profile picture upload
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        Console.WriteLine($"Processing profile picture: {profilePicture.FileName}, Size: {profilePicture.Length} bytes");
                        
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["Error"] = "Only JPG, PNG, and GIF files are allowed.";
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
                        
                        Console.WriteLine($"Saving to: {filePath}");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(stream);
                        }
                        
                        // Verify file was saved
                        if (!System.IO.File.Exists(filePath))
                        {
                            TempData["Error"] = "Failed to save profile picture.";
                            return View(profile);
                        }

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
                        Console.WriteLine($"Profile picture path set to: {existingProfile.ProfilePicturePath}");
                    }

                    // Save changes
                    _context.Update(existingProfile);
                    var result = await _context.SaveChangesAsync();
                    Console.WriteLine($"SaveChanges result: {result} rows affected");
                    
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating profile: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["Error"] = $"An error occurred while updating your profile: {ex.Message}";
                }
            }

            return View(profile);
        }
    }
}