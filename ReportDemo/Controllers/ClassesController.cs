using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services.Interfaces;

namespace ReportDemo.Controllers
{
    [Authorize] // Require login for all actions in this controller
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPromotionService _promotionService;

        public ClassesController(ApplicationDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        // GET: Classes
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewBag.ClassNameSortParm = string.IsNullOrEmpty(sortOrder) ? "classname_desc" : "";
            ViewBag.SectionSortParm = sortOrder == "Section" ? "section_desc" : "Section";
            ViewBag.TeacherSortParm = sortOrder == "Teacher" ? "teacher_desc" : "Teacher";
            ViewBag.CurrentFilter = searchString;

            var classes = from c in _context.Classes.Include(c => c.Students)
                         select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(c => c.ClassName.Contains(searchString)
                                           || c.Section.Contains(searchString)
                                           || c.TeacherInCharge!.Contains(searchString)
                                           || c.RoomNumber!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "classname_desc":
                    classes = classes.OrderByDescending(c => c.ClassName);
                    break;
                case "Section":
                    classes = classes.OrderBy(c => c.Section);
                    break;
                case "section_desc":
                    classes = classes.OrderByDescending(c => c.Section);
                    break;
                case "Teacher":
                    classes = classes.OrderBy(c => c.TeacherInCharge);
                    break;
                case "teacher_desc":
                    classes = classes.OrderByDescending(c => c.TeacherInCharge);
                    break;
                default:
                    classes = classes.OrderBy(c => c.ClassName).ThenBy(c => c.Section);
                    break;
            }

            return View(await classes.AsNoTracking().ToListAsync());
        }

        // GET: Classes/ManageClasses - Dedicated management page with card layout
        public async Task<IActionResult> ManageClasses(string searchString, string sortOrder)
        {
            ViewBag.ClassNameSortParm = string.IsNullOrEmpty(sortOrder) ? "classname_desc" : "";
            ViewBag.SectionSortParm = sortOrder == "Section" ? "section_desc" : "Section";
            ViewBag.TeacherSortParm = sortOrder == "Teacher" ? "teacher_desc" : "Teacher";
            ViewBag.CurrentFilter = searchString;

            var classes = from c in _context.Classes.Include(c => c.Students)
                         select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(c => c.ClassName.Contains(searchString)
                                           || c.Section.Contains(searchString)
                                           || c.TeacherInCharge!.Contains(searchString)
                                           || c.RoomNumber!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "classname_desc":
                    classes = classes.OrderByDescending(c => c.ClassName);
                    break;
                case "Section":
                    classes = classes.OrderBy(c => c.Section);
                    break;
                case "section_desc":
                    classes = classes.OrderByDescending(c => c.Section);
                    break;
                case "Teacher":
                    classes = classes.OrderBy(c => c.TeacherInCharge);
                    break;
                case "teacher_desc":
                    classes = classes.OrderByDescending(c => c.TeacherInCharge);
                    break;
                default:
                    classes = classes.OrderBy(c => c.ClassName).ThenBy(c => c.Section);
                    break;
            }

            return View(await classes.AsNoTracking().ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassName,Section,TeacherInCharge,RoomNumber")] Class @class)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate ClassName + Section combination
                var existingClass = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassName == @class.ClassName && c.Section == @class.Section);

                if (existingClass != null)
                {
                    ModelState.AddModelError("", "A class with this name and section already exists.");
                    return View(@class);
                }

                @class.CreatedAt = DateTime.UtcNow;
                @class.UpdatedAt = DateTime.UtcNow;

                _context.Add(@class);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Class {@class.DisplayName} has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            return View(@class);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClassName,Section,TeacherInCharge,RoomNumber,CreatedAt")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate ClassName + Section combination (excluding current record)
                    var existingClass = await _context.Classes
                        .FirstOrDefaultAsync(c => c.ClassName == @class.ClassName && c.Section == @class.Section && c.Id != @class.Id);

                    if (existingClass != null)
                    {
                        ModelState.AddModelError("", "A class with this name and section already exists.");
                        return View(@class);
                    }

                    @class.UpdatedAt = DateTime.UtcNow;
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Class {@class.DisplayName} has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id);
            
            if (@class != null)
            {
                // Check if class has students
                if (@class.Students.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete class {@class.DisplayName} because it has {@class.Students.Count} student(s) assigned to it. Please reassign or remove the students first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Class {@class.DisplayName} has been deleted successfully.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Classes/Students/5 - View students in a specific class
        public async Task<IActionResult> Students(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@class == null)
            {
                return NotFound();
            }

            ViewBag.Class = @class;
            return View(@class.Students.ToList());
        }

        // GET: Classes/Promote/5 - Check promotion eligibility and show promotion form
        public async Task<IActionResult> Promote(int? id)
        {
            if (id == null) return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@class == null) return NotFound();

            var currentAcademicYear = GetCurrentAcademicYear();
            var canPromote = await _promotionService.CanPromoteClassAsync(id.Value, currentAcademicYear, "Final");
            var nextClass = await _promotionService.GetNextClassAsync(id.Value);
            var isMatricClass = await _promotionService.IsMatricClassAsync(id.Value);

            ViewBag.CanPromote = canPromote;
            ViewBag.NextClass = nextClass;
            ViewBag.IsMatricClass = isMatricClass;
            ViewBag.AcademicYear = currentAcademicYear;

            return View(@class);
        }

        // POST: Classes/Promote/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promote(int id, string academicYear)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            var promotedBy = User?.Identity?.Name ?? "System";
            var result = await _promotionService.PromoteStudentsAsync(id, academicYear, promotedBy);

            if (result.Success)
            {
                TempData["SuccessMessage"] = string.Join(" ", result.Messages);
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Classes/PromotionStatus/5
        public async Task<IActionResult> PromotionStatus(int? id)
        {
            if (id == null) return NotFound();

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            var currentAcademicYear = GetCurrentAcademicYear();
            var canPromote = await _promotionService.CanPromoteClassAsync(id.Value, currentAcademicYear, "Final");

            return Json(new { canPromote, academicYear = currentAcademicYear });
        }

        // GET: Classes/BulkPromote/5 - Promote all students in class after exam completion
        public async Task<IActionResult> BulkPromote(int? id)
        {
            if (id == null) return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@class == null) return NotFound();

            var currentAcademicYear = GetCurrentAcademicYear();
            var canPromote = await _promotionService.CanPromoteClassAsync(id.Value, currentAcademicYear, "Final");
            var nextClass = await _promotionService.GetNextClassAsync(id.Value);
            var isMatricClass = await _promotionService.IsMatricClassAsync(id.Value);

            // Get exam results for this class
            var examResults = await _context.ExamResults
                .Include(e => e.Student)
                .Where(e => e.ClassId == id && e.AcademicYear == currentAcademicYear && e.Term == "Final")
                .ToListAsync();

            ViewBag.CanPromote = canPromote;
            ViewBag.NextClass = nextClass;
            ViewBag.IsMatricClass = isMatricClass;
            ViewBag.AcademicYear = currentAcademicYear;
            ViewBag.ExamResults = examResults;
            ViewBag.CurrentClass = @class;

            // Calculate promotion statistics
            var totalStudents = @class.Students.Count;
            var studentsWithResults = examResults.Count;
            var passedStudents = examResults.Count(e => e.IsPassed);
            var failedStudents = examResults.Count(e => !e.IsPassed);

            ViewBag.TotalStudents = totalStudents;
            ViewBag.StudentsWithResults = studentsWithResults;
            ViewBag.PassedStudents = passedStudents;
            ViewBag.FailedStudents = failedStudents;
            ViewBag.StudentsWithoutResults = totalStudents - studentsWithResults;

            return View(@class);
        }

        // POST: Classes/BulkPromote/5 - Execute bulk promotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkPromote(int id, string academicYear, bool confirmPromotion = false)
        {
            if (!confirmPromotion)
            {
                TempData["ErrorMessage"] = "Please confirm the promotion by checking the confirmation box.";
                return RedirectToAction(nameof(BulkPromote), new { id });
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();

            var promotedBy = User?.Identity?.Name ?? "System";
            var result = await _promotionService.PromoteStudentsAsync(id, academicYear, promotedBy);

            if (result.Success)
            {
                var message = string.Join(" ", result.Messages);
                if (result.GraduatedCount > 0)
                {
                    message += $" 🎓 {result.GraduatedCount} students graduated!";
                }
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Classes/ManageStudents - Class-wise student management
        public async Task<IActionResult> ManageStudents(int? classId, string searchString, string sortOrder)
        {
            // Get all classes for dropdown
            ViewBag.Classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .ToListAsync();
            
            ViewBag.CurrentClassId = classId;
            ViewBag.CurrentSearch = searchString;
            
            // Sorting parameters
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.RollSortParm = sortOrder == "roll" ? "roll_desc" : "roll";
            ViewBag.AgeSortParm = sortOrder == "age" ? "age_desc" : "age";
            
            var students = from s in _context.Students.Include(s => s.Class)
                          select s;

            // Filter by class if selected
            if (classId.HasValue && classId > 0)
            {
                students = students.Where(s => s.ClassId == classId);
                
                // Get selected class info
                var selectedClass = await _context.Classes.FindAsync(classId);
                ViewBag.SelectedClass = selectedClass;
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FirstName.Contains(searchString)
                                             || s.LastName.Contains(searchString)
                                             || s.RollNumber.Contains(searchString)
                                             || s.Email.Contains(searchString));
            }

            // Sorting
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName);
                    break;
                case "roll":
                    students = students.OrderBy(s => s.RollNumber);
                    break;
                case "roll_desc":
                    students = students.OrderByDescending(s => s.RollNumber);
                    break;
                case "age":
                    students = students.OrderBy(s => s.DateOfBirth);
                    break;
                case "age_desc":
                    students = students.OrderByDescending(s => s.DateOfBirth);
                    break;
                default:
                    students = students.OrderBy(s => s.FirstName).ThenBy(s => s.LastName);
                    break;
            }

            var studentList = await students.ToListAsync();
            
            // Calculate stats for selected class
            if (classId.HasValue && classId > 0)
            {
                ViewBag.TotalStudents = studentList.Count;
                ViewBag.MaleStudents = studentList.Count(s => s.Gender.ToLower() == "male");
                ViewBag.FemaleStudents = studentList.Count(s => s.Gender.ToLower() == "female");
                ViewBag.AverageAge = studentList.Any() ? studentList.Average(s => s.Age) : 0;
            }
            
            return View(studentList);
        }

        private string GetCurrentAcademicYear()
        {
            var currentDate = DateTime.Now;
            var startYear = currentDate.Month >= 4 ? currentDate.Year : currentDate.Year - 1;
            return $"{startYear}-{startYear + 1:D2}";
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}