using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services;
using System.Text;

namespace ReportDemo.Controllers
{
    [Authorize]
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPromotionService _promotionService;

        public PromotionController(ApplicationDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        public async Task<IActionResult> Dashboard(int? classId, string? academicYear)
        {
            var classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();

            ViewBag.Classes = classes;
            ViewBag.CurrentAcademicYear = academicYear ?? GetCurrentAcademicYear();
            ViewBag.SelectedClassId = classId;

            List<StudentEligibilityVM> students = new();
            if (classId.HasValue)
            {
                var selectedClass = await _context.Classes
                    .Include(c => c.Students)
                    .FirstOrDefaultAsync(c => c.Id == classId.Value);
                if (selectedClass != null)
                {
                    ViewBag.Class = selectedClass;
                    var ay = ViewBag.CurrentAcademicYear as string;
                    var results = await _context.ExamResults
                        .Where(e => e.ClassId == classId.Value && e.AcademicYear == ay && e.Term == "Final")
                        .ToListAsync();

                    foreach (var s in selectedClass.Students.OrderBy(s => s.RollNumber))
                    {
                        var r = results.Where(e => e.StudentId == s.Id)
                                       .OrderByDescending(e => e.ExamDate)
                                       .FirstOrDefault();
                        var status = ComputeEligibility(r);
                        students.Add(new StudentEligibilityVM
                        {
                            Student = s,
                            Result = r,
                            Eligibility = status
                        });
                    }

                    var canPromote = await _promotionService.CanPromoteClassAsync(classId.Value, ay!, "Final");
                    ViewBag.CanPromote = canPromote;
                    ViewBag.NextClass = await _promotionService.GetNextClassAsync(classId.Value);
                }
            }

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int classId, string academicYear)
        {
            var canPromote = await _promotionService.CanPromoteClassAsync(classId, academicYear, "Final");
            var nextClass = await _promotionService.GetNextClassAsync(classId);
            var isMatric = await _promotionService.IsMatricClassAsync(classId);

            return Json(new { canPromote, nextClass = nextClass?.DisplayName, isMatric });
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
            return RedirectToAction(nameof(Dashboard), new { classId, academicYear });
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
