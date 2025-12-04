using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services.Interfaces;  // This is the correct namespace for IPromotionService
using ReportDemo.ViewModels.Promotion;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ReportDemo.Controllers
{
    // [Authorize(Roles = "Admin,Teacher")] // Temporarily commented out for testing
    [Route("promotion")]
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(
            ApplicationDbContext context, 
            IPromotionService promotionService,
            ILogger<PromotionController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("")]
        [HttpGet("index")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _promotionService.InitializePromotionViewModelAsync();
                return View("PromotionDashboard", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing promotion dashboard");
                TempData["ErrorMessage"] = "An error occurred while initializing the promotion dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudentsForPromotion(int currentSessionId, int currentClassId, int? currentSectionId = null)
        {
            try
            {
                var viewModel = await _promotionService.GetStudentsForPromotionAsync(
                    currentSessionId, currentClassId, currentSectionId);
                
                return PartialView("_StudentPromotionList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students for promotion");
                return BadRequest(new { success = false, message = "Error loading students. Please try again." });
            }
        }

        [HttpPost("process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPromotion([FromBody] PromotionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            try
            {
                var result = await _promotionService.ProcessPromotionAsync(request);
                
                if (result.Success)
                {
                    _logger.LogInformation($"Successfully promoted {result.PromotedStudents?.Count ?? 0} students");
                    return Json(new { 
                        success = true, 
                        message = $"Successfully promoted {result.PromotedStudents?.Count ?? 0} students.",
                        data = new { promotedCount = result.PromotedStudents?.Count ?? 0 }
                    });
                }
                
                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing promotion");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while processing the promotion. Please try again." 
                });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPromotionHistory(
            int? studentId = null, 
            int? classId = null, 
            int? sessionId = null,
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            try
            {
                var history = await _promotionService.GetPromotionHistoryAsync(
                    studentId, classId, sessionId, fromDate, toDate);
                
                return PartialView("_PromotionHistoryList", history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving promotion history");
                return BadRequest(new { success = false, message = "Error loading promotion history." });
            }
        }

        [HttpGet("check-eligibility/{studentId}")]
        public async Task<IActionResult> CheckEligibility(int studentId, int currentSessionId, int currentClassId)
        {
            try
            {
                var isEligible = await _promotionService.IsStudentEligibleForPromotionAsync(
                    studentId, currentSessionId, currentClassId);
                
                return Json(new { success = true, isEligible });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking eligibility for student {studentId}");
                return Json(new { success = false, message = "Error checking eligibility." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportSummary(int classId, string academicYear)
        {
            var classInfo = await _context.Classes.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == classId);
            if (classInfo == null) return NotFound();

            var results = await _context.ExamResults
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && e.AcademicYear == academicYear && e.Term == "Final")
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("RollNo,FullName,Percentage,Grade,Passed,Eligibility");
            foreach (var s in classInfo.Students.OrderBy(s => s.RollNumber))
            {
                var r = results.Where(e => e.StudentId == s.Id).OrderByDescending(e => e.ExamDate).FirstOrDefault();
                var eligibility = ComputeEligibility(r);
                sb.AppendLine($"{s.RollNumber},{Escape(s.FullName)},{r?.Percentage},{r?.Grade},{r?.IsPassed},{eligibility}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Promotion_Summary_{classId}_{academicYear}.csv";
            return File(bytes, "text/csv", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteAll(int classId, string academicYear)
        {
            var promotedBy = User?.Identity?.Name ?? "System";
            var result = await _promotionService.PromoteStudentsAsync(classId, academicYear, promotedBy);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = string.Join(" ", result.Messages);
            return RedirectToAction(nameof(Index));
        }

        private static string Escape(string? input) => string.IsNullOrWhiteSpace(input) ? "" : input.Replace(",", " ");

        private string ComputeEligibility(ExamResult? r)
        {
            if (r == null) return "Not Eligible";
            if (!r.ExamCompleted) return "Not Eligible";
            if (r.Percentage >= 40) return "Eligible";
            if (r.Percentage >= 35) return "Conditional";
            return "Not Eligible";
        }

        private string GetCurrentAcademicYear()
        {
            var now = DateTime.Now;
            var start = now.Month >= 4 ? now.Year : now.Year - 1;
            return $"{start}-{(start + 1):D2}";
        }
    }

    public class StudentEligibilityVM
    {
        public Student Student { get; set; } = null!;
        public ExamResult? Result { get; set; }
        public string Eligibility { get; set; } = "Not Eligible";
    }
}
