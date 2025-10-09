using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services;

namespace ReportDemo.Controllers
{
    [Authorize]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPromotionService _promotionService;

        public ExamController(ApplicationDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        // ------------------- Index - Show all exams ------------------- //
        public async Task<IActionResult> Index(string academicYear, int? classId)
        {
            ViewBag.Classes = await _context.Classes.OrderBy(c => c.ClassName).ThenBy(c => c.Section).ToListAsync();
            ViewBag.CurrentAcademicYear = academicYear ?? GetCurrentAcademicYear();
            ViewBag.CurrentClassId = classId;

            var examsQuery = _context.ExamResults
                .Include(e => e.Student)
                .Include(e => e.Class)
                .AsQueryable();

            if (!string.IsNullOrEmpty(academicYear))
            {
                examsQuery = examsQuery.Where(e => e.AcademicYear == academicYear);
            }

            if (classId.HasValue)
            {
                examsQuery = examsQuery.Where(e => e.ClassId == classId);
            }

            var exams = await examsQuery.OrderByDescending(e => e.ExamDate).ToListAsync();
            return View(exams);
        }

        // ------------------- Class Exam Status - Show exam status for a class ------------------- //
        public async Task<IActionResult> ClassExamStatus(int classId, string academicYear = null)
        {
            academicYear ??= GetCurrentAcademicYear();

            var classInfo = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
                return NotFound();

            var examResults = await _context.ExamResults
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && e.AcademicYear == academicYear)
                .ToListAsync();

            ViewBag.Class = classInfo;
            ViewBag.AcademicYear = academicYear;
            ViewBag.ExamResults = examResults;

            // Calculate statistics
            var totalStudents = classInfo.Students.Count;
            var studentsWithResults = examResults.Select(e => e.StudentId).Distinct().Count();
            var passedStudents = examResults.Where(e => e.IsPassed).Select(e => e.StudentId).Distinct().Count();
            
            ViewBag.TotalStudents = totalStudents;
            ViewBag.StudentsWithResults = studentsWithResults;
            ViewBag.PassedStudents = passedStudents;
            ViewBag.CompletionRate = totalStudents > 0 ? (studentsWithResults * 100 / totalStudents) : 0;
            ViewBag.PassRate = studentsWithResults > 0 ? (passedStudents * 100 / studentsWithResults) : 0;

            // Check if class is ready for promotion
            var canPromote = await _promotionService.CanPromoteClassAsync(classId, academicYear, "Final");
            ViewBag.CanPromote = canPromote;

            return View(classInfo.Students.ToList());
        }

        // ------------------- Create - GET ------------------- //
        public async Task<IActionResult> Create()
        {
            ViewBag.Students = await _context.Students
                .Include(s => s.Class)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();
            
            ViewBag.Classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();
            
            var examResult = new ExamResult
            {
                AcademicYear = GetCurrentAcademicYear(),
                Term = "Final",
                ExamDate = DateTime.UtcNow
            };
            
            return View(examResult);
        }
        
        // ------------------- Create - POST ------------------- //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamResult examResult)
        {
            if (ModelState.IsValid)
            {
                // Calculate grade based on percentage
                examResult.Grade = CalculateGrade(examResult.Percentage);
                examResult.IsPassed = examResult.Percentage >= 40; // 40% passing grade
                examResult.ExamCompleted = true;
                examResult.CreatedAt = DateTime.UtcNow;
                examResult.UpdatedAt = DateTime.UtcNow;

                // Check if result already exists
                var existingResult = await _context.ExamResults
                    .FirstOrDefaultAsync(e => e.StudentId == examResult.StudentId 
                                            && e.ClassId == examResult.ClassId 
                                            && e.AcademicYear == examResult.AcademicYear 
                                            && e.Term == examResult.Term);

                if (existingResult != null)
                {
                    ModelState.AddModelError("", "Exam result already exists for this student in the selected term and academic year.");
                }
                else
                {
                    _context.Add(examResult);
                    await _context.SaveChangesAsync();

                    var student = await _context.Students.FindAsync(examResult.StudentId);
                    TempData["SuccessMessage"] = $"Exam result recorded successfully for {student?.FullName}. Grade: {examResult.Grade} ({examResult.Percentage}%)"; 
                    
                    return RedirectToAction(nameof(Index));
                }
            }

            // Repopulate ViewBag on error
            ViewBag.Students = await _context.Students
                .Include(s => s.Class)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();
            
            ViewBag.Classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();
            
            return View(examResult);
        }

        // ------------------- Record Exam Result ------------------- //
        public async Task<IActionResult> RecordResult(int studentId, int classId)
        {
            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return NotFound();

            var examResult = new ExamResult
            {
                StudentId = studentId,
                ClassId = classId,
                AcademicYear = GetCurrentAcademicYear(),
                Term = "Final",
                ExamDate = DateTime.UtcNow
            };

            ViewBag.Student = student;
            return View(examResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(ExamResult examResult)
        {
            if (ModelState.IsValid)
            {
                // Calculate grade based on percentage
                examResult.Grade = CalculateGrade(examResult.Percentage);
                examResult.IsPassed = examResult.Percentage >= 40; // 40% passing grade
                examResult.ExamCompleted = true;
                examResult.ExamDate = DateTime.UtcNow;

                // Check if result already exists
                var existingResult = await _context.ExamResults
                    .FirstOrDefaultAsync(e => e.StudentId == examResult.StudentId 
                                            && e.ClassId == examResult.ClassId 
                                            && e.AcademicYear == examResult.AcademicYear 
                                            && e.Term == examResult.Term);

                if (existingResult != null)
                {
                    // Update existing result
                    existingResult.Percentage = examResult.Percentage;
                    existingResult.Grade = examResult.Grade;
                    existingResult.IsPassed = examResult.IsPassed;
                    existingResult.ExamCompleted = true;
                    existingResult.ExamDate = DateTime.UtcNow;
                    existingResult.Remarks = examResult.Remarks;
                    
                    _context.Update(existingResult);
                }
                else
                {
                    // Create new result
                    _context.Add(examResult);
                }

                await _context.SaveChangesAsync();

                var student = await _context.Students.FindAsync(examResult.StudentId);
                TempData["SuccessMessage"] = $"Exam result recorded successfully for {student?.FullName}. Grade: {examResult.Grade} ({examResult.Percentage}%)";
                
                return RedirectToAction("ClassExamStatus", new { classId = examResult.ClassId });
            }

            var studentInfo = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == examResult.StudentId);
            ViewBag.Student = studentInfo;
            
            return View(examResult);
        }

        // ------------------- Bulk Record Results ------------------- //
        public async Task<IActionResult> BulkRecordResults(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
                return NotFound();

            var academicYear = GetCurrentAcademicYear();
            var existingResults = await _context.ExamResults
                .Where(e => e.ClassId == classId && e.AcademicYear == academicYear && e.Term == "Final")
                .ToListAsync();

            var studentsWithResults = new List<ExamResult>();

            foreach (var student in classInfo.Students.OrderBy(s => s.RollNumber))
            {
                var existingResult = existingResults.FirstOrDefault(e => e.StudentId == student.Id);
                if (existingResult != null)
                {
                    studentsWithResults.Add(existingResult);
                }
                else
                {
                    studentsWithResults.Add(new ExamResult
                    {
                        StudentId = student.Id,
                        Student = student,
                        ClassId = classId,
                        AcademicYear = academicYear,
                        Term = "Final",
                        ExamDate = DateTime.UtcNow
                    });
                }
            }

            ViewBag.Class = classInfo;
            ViewBag.AcademicYear = academicYear;
            
            return View(studentsWithResults);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRecordResults(List<ExamResult> examResults, int classId)
        {
            if (ModelState.IsValid)
            {
                var academicYear = GetCurrentAcademicYear();
                var successCount = 0;

                foreach (var result in examResults)
                {
                    if (result.Percentage >= 0) // Only process if percentage is provided
                    {
                        result.Grade = CalculateGrade(result.Percentage);
                        result.IsPassed = result.Percentage >= 40;
                        result.ExamCompleted = true;
                        result.ExamDate = DateTime.UtcNow;
                        result.AcademicYear = academicYear;
                        result.Term = "Final";
                        result.ClassId = classId;

                        var existingResult = await _context.ExamResults
                            .FirstOrDefaultAsync(e => e.StudentId == result.StudentId 
                                                    && e.ClassId == classId 
                                                    && e.AcademicYear == academicYear 
                                                    && e.Term == "Final");

                        if (existingResult != null)
                        {
                            existingResult.Percentage = result.Percentage;
                            existingResult.Grade = result.Grade;
                            existingResult.IsPassed = result.IsPassed;
                            existingResult.ExamCompleted = true;
                            existingResult.ExamDate = DateTime.UtcNow;
                            existingResult.Remarks = result.Remarks;
                            _context.Update(existingResult);
                        }
                        else
                        {
                            _context.Add(result);
                        }
                        
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully recorded exam results for {successCount} students.";
                }

                return RedirectToAction("ClassExamStatus", new { classId = classId });
            }

            var classInfo = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);
            ViewBag.Class = classInfo;
            ViewBag.AcademicYear = GetCurrentAcademicYear();
            
            return View(examResults);
        }

        // ------------------- Helper Methods ------------------- //
        private string GetCurrentAcademicYear()
        {
            var currentDate = DateTime.Now;
            var startYear = currentDate.Month >= 4 ? currentDate.Year : currentDate.Year - 1;
            return $"{startYear}-{startYear + 1:D2}";
        }

        private string CalculateGrade(double percentage)
        {
            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B",
                >= 60 => "C",
                >= 40 => "D",
                _ => "F"
            };
        }
        
        // ------------------- Details ------------------- //
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examResult = await _context.ExamResults
                .Include(e => e.Student)
                .Include(e => e.Class)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (examResult == null)
            {
                return NotFound();
            }

            return View(examResult);
        }
        
        // ------------------- Edit - GET ------------------- //
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examResult = await _context.ExamResults.FindAsync(id);
            if (examResult == null)
            {
                return NotFound();
            }
            
            ViewBag.Students = await _context.Students
                .Include(s => s.Class)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();
            
            ViewBag.Classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();
                
            return View(examResult);
        }

        // ------------------- Edit - POST ------------------- //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamResult examResult)
        {
            if (id != examResult.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate grade based on percentage
                    examResult.Grade = CalculateGrade(examResult.Percentage);
                    examResult.IsPassed = examResult.Percentage >= 40;
                    examResult.ExamCompleted = true;
                    examResult.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(examResult);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Exam result updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = examResult.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamResultExists(examResult.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            ViewBag.Students = await _context.Students
                .Include(s => s.Class)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();
            
            ViewBag.Classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();
                
            return View(examResult);
        }

        // ------------------- Delete - POST ------------------- //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var examResult = await _context.ExamResults.FindAsync(id);
            if (examResult != null)
            {
                _context.ExamResults.Remove(examResult);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Exam result deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Exam result not found.";
            }

            return RedirectToAction(nameof(Index));
        }
        
        private bool ExamResultExists(int id)
        {
            return _context.ExamResults.Any(e => e.Id == id);
        }
        
        
        // ------------------- Exam Reports ------------------- //
        public async Task<IActionResult> ExamReports()
        {
            // For now, redirect to Index - you can implement detailed reports later
            TempData["InfoMessage"] = "Exam reports feature coming soon! Currently showing all exam results.";
            return RedirectToAction(nameof(Index));
        }
    }
}
