using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.ViewModels;
using System.Diagnostics;

namespace ReportDemo.Controllers
{
    [Authorize] // Require login for all actions in this controller
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get comprehensive statistics for dashboard
            var dashboardData = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalTeachers = await _context.Classes
                    .Where(c => !string.IsNullOrEmpty(c.TeacherInCharge))
                    .Select(c => c.TeacherInCharge)
                    .Distinct()
                    .CountAsync(),
                
                // Recent enrollments (last 30 days)
                RecentEnrollments = await _context.Students
                    .Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync(),

                // Class-wise statistics
                ClassStats = await _context.Classes
                    .Include(c => c.Students)
                    .Select(c => new ClassStatistic
                    {
                        ClassName = c.ClassName,
                        Section = c.Section,
                        StudentCount = c.Students.Count,
                        TeacherInCharge = c.TeacherInCharge ?? "Not Assigned"
                    })
                    .OrderBy(c => c.ClassName)
                    .ThenBy(c => c.Section)
                    .ToListAsync(),

                // Recent students (last 10)
                RecentStudents = await _context.Students
                    .Include(s => s.Class)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(10)
                    .Select(s => new RecentStudentViewModel
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        RollNumber = s.RollNumber,
                        ClassName = s.Class != null ? s.Class.ClassName : "No Class",
                        Section = s.Class != null ? s.Class.Section : "",
                        EnrolledDate = s.CreatedAt,
                        Age = s.Age
                    })
                    .ToListAsync(),

                // Age distribution - using client evaluation
                AgeDistribution = await GetAgeDistributionAsync(),

                // Gender distribution
                GenderDistribution = await _context.Students
                    .Where(s => !string.IsNullOrEmpty(s.Gender))
                    .GroupBy(s => s.Gender)
                    .Select(g => new GenderDistribution
                    {
                        Gender = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(),

                // Get classes with no students
                EmptyClasses = await _context.Classes
                    .Include(c => c.Students)
                    .Where(c => c.Students.Count == 0)
                    .CountAsync(),

                // Average students per class
                AvgStudentsPerClass = await _context.Classes
                    .Include(c => c.Students)
                    .AverageAsync(c => c.Students.Count)
            };

            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Quick Actions API endpoints for dashboard
        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            var stats = new
            {
                studentsToday = await _context.Students
                    .Where(s => s.CreatedAt.Date == DateTime.UtcNow.Date)
                    .CountAsync(),
                
                studentsThisWeek = await _context.Students
                    .Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync(),
                
                studentsThisMonth = await _context.Students
                    .Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync(),
                
                activeClasses = await _context.Classes
                    .Include(c => c.Students)
                    .Where(c => c.Students.Count > 0)
                    .CountAsync()
            };

            return Json(stats);
        }

        private async Task<List<AgeDistribution>> GetAgeDistributionAsync()
        {
            // Get all students with their birth dates and calculate age on the client side
            var students = await _context.Students
                .Select(s => s.DateOfBirth)
                .ToListAsync();

            var ageGroups = students
                .Select(dob => CalculateAge(dob))
                .GroupBy(age => age < 10 ? "Under 10" :
                               age < 15 ? "10-14" :
                               age < 18 ? "15-17" : "18+")
                .Select(g => new AgeDistribution
                {
                    AgeGroup = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.AgeGroup)
                .ToList();

            return ageGroups;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }
}
