using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using AspNetCore.Reporting;
using System.Data;

namespace ReportDemo.Controllers
{
    [Authorize] // ✅ Require login for all actions in this controller
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------- Index ------------------- //
        public async Task<IActionResult> Index()
        {
            var students = await _context.Students.ToListAsync();
            return View(students);
        }

        // ------------------- Details ------------------- //
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // ------------------- Create ------------------- //
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // ------------------- Edit ------------------- //
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Id == student.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // ------------------- Delete ------------------- //
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ------------------- RDLC Report ------------------- //
        public IActionResult StudentRDLCReport()
        {
            var students = _context.Students.AsNoTracking().ToList();

            // ✅ Build DataTable for RDLC
            DataTable dt = new DataTable("Student");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Age", typeof(int));

            foreach (var s in students)
            {
                dt.Rows.Add(s.Id, s.Name, s.Age);
            }

            string rdlcPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "Reports", "rdStudent.rdlc");

            if (!System.IO.File.Exists(rdlcPath))
            {
                throw new FileNotFoundException($"❌ RDLC file not found at path: {rdlcPath}");
            }

            LocalReport report = new LocalReport(rdlcPath);

            // ✅ Match dataset name inside RDLC file (check in Visual Studio RDLC designer)
            report.AddDataSource("dsStudent", dt);

            var result = report.Execute(RenderType.Pdf, 1, null);

            return File(result.MainStream, "application/pdf", "StudentReport.pdf");
        }
    }
}
