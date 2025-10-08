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
            try
            {
                var students = _context.Students.AsNoTracking().ToList();

                if (!students.Any())
                {
                    TempData["Message"] = "No student records found to generate report.";
                    return RedirectToAction(nameof(Index));
                }

                // Create a simple PDF report using QuestPDF (which is already in your project)
                return GenerateStudentPDF(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ------------------- Generate Professional Report ------------------- //
        private IActionResult GenerateStudentPDF(List<Student> students)
        {
            // Create a professional HTML report that can be printed as PDF
            return GenerateProfessionalReport(students);
        }
        
        // ------------------- Generate Professional HTML Report ------------------- //
        private IActionResult GenerateProfessionalReport(List<Student> students)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Student Management System Report</title>
    <style>
        @media print {{
            body {{ -webkit-print-color-adjust: exact; }}
            .no-print {{ display: none; }}
            .page-break {{ page-break-after: always; }}
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background: #f8f9fa;
            color: #2c3e50;
        }}
        
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        
        .header {{
            text-align: center;
            margin-bottom: 40px;
            border-bottom: 3px solid #3498db;
            padding-bottom: 20px;
        }}
        
        .header h1 {{
            color: #2c3e50;
            font-size: 2.5rem;
            margin: 0;
            font-weight: 700;
        }}
        
        .header .subtitle {{
            color: #7f8c8d;
            font-size: 1.1rem;
            margin-top: 10px;
        }}
        
        .report-info {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }}
        
        .info-card {{
            background: linear-gradient(135deg, #3498db, #2980b9);
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }}
        
        .info-card.success {{
            background: linear-gradient(135deg, #27ae60, #2ecc71);
        }}
        
        .info-card.warning {{
            background: linear-gradient(135deg, #f39c12, #e67e22);
        }}
        
        .info-card.danger {{
            background: linear-gradient(135deg, #e74c3c, #c0392b);
        }}
        
        .info-card h3 {{
            font-size: 2rem;
            margin: 0;
            font-weight: 700;
        }}
        
        .info-card p {{
            margin: 8px 0 0 0;
            font-size: 0.9rem;
            opacity: 0.9;
        }}
        
        .data-section {{
            margin-top: 30px;
        }}
        
        .section-title {{
            font-size: 1.5rem;
            color: #2c3e50;
            margin-bottom: 20px;
            font-weight: 600;
            border-left: 4px solid #3498db;
            padding-left: 15px;
        }}
        
        .students-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            border-radius: 8px;
            overflow: hidden;
        }}
        
        .students-table thead {{
            background: linear-gradient(135deg, #34495e, #2c3e50);
            color: white;
        }}
        
        .students-table th,
        .students-table td {{
            padding: 15px 20px;
            text-align: left;
            border-bottom: 1px solid #ecf0f1;
        }}
        
        .students-table th {{
            font-weight: 600;
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        
        .students-table tbody tr {{
            transition: background-color 0.3s ease;
        }}
        
        .students-table tbody tr:hover {{
            background-color: #f8f9fa;
        }}
        
        .students-table tbody tr:nth-child(even) {{
            background-color: #f8f9fa;
        }}
        
        .student-id {{
            font-weight: 600;
            color: #3498db;
            font-family: 'Courier New', monospace;
        }}
        
        .student-name {{
            font-weight: 500;
            color: #2c3e50;
        }}
        
        .age-badge {{
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 500;
        }}
        
        .age-adult {{
            background: #d5f4e6;
            color: #27ae60;
        }}
        
        .age-minor {{
            background: #fdeaa7;
            color: #f39c12;
        }}
        
        .footer {{
            margin-top: 40px;
            text-align: center;
            color: #7f8c8d;
            font-size: 0.9rem;
            border-top: 1px solid #ecf0f1;
            padding-top: 20px;
        }}
        
        .print-button {{
            position: fixed;
            top: 20px;
            right: 20px;
            background: #3498db;
            color: white;
            border: none;
            padding: 15px 25px;
            border-radius: 8px;
            font-size: 0.9rem;
            font-weight: 500;
            cursor: pointer;
            z-index: 1000;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }}
        
        .print-button:hover {{
            background: #2980b9;
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        }}
    </style>
</head>
<body>
    <button class='print-button no-print' onclick='window.print()'>📄 Print / Save as PDF</button>
    
    <div class='container'>
        <div class='header'>
            <h1>📊 Student Management System</h1>
            <div class='subtitle'>Comprehensive Student Report</div>
            <div class='subtitle'>Generated on {DateTime.Now:dddd, dd MMMM yyyy 'at' HH:mm}</div>
        </div>
        
        <div class='report-info'>
            <div class='info-card'>
                <h3>{students.Count}</h3>
                <p>Total Students</p>
            </div>
            <div class='info-card success'>
                <h3>{students.Count(s => s.Age >= 18)}</h3>
                <p>Adult Students</p>
            </div>
            <div class='info-card warning'>
                <h3>{students.Count(s => s.Age < 18)}</h3>
                <p>Minor Students</p>
            </div>
            <div class='info-card danger'>
                <h3>{(students.Any() ? students.Average(s => s.Age) : 0):F1}</h3>
                <p>Average Age</p>
            </div>
        </div>
        
        <div class='data-section'>
            <h2 class='section-title'>📋 Student Records</h2>";

            if (students.Any())
            {
                html += @"
            <table class='students-table'>
                <thead>
                    <tr>
                        <th>Student ID</th>
                        <th>Full Name</th>
                        <th>Age</th>
                        <th>Category</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>";

                foreach (var student in students)
                {
                    var category = student.Age >= 18 ? "Adult" : "Minor";
                    var categoryClass = student.Age >= 18 ? "age-adult" : "age-minor";
                    html += $@"
                    <tr>
                        <td><span class='student-id'>#{student.Id:D4}</span></td>
                        <td><span class='student-name'>{student.Name ?? "N/A"}</span></td>
                        <td>{student.Age} years</td>
                        <td><span class='age-badge {categoryClass}'>{category}</span></td>
                        <td>✅ Active</td>
                    </tr>";
                }

                html += @"
                </tbody>
            </table>";
            }
            else
            {
                html += @"
            <div style='text-align: center; padding: 60px; color: #7f8c8d;'>
                <h3>No Student Records Found</h3>
                <p>There are currently no students in the system.</p>
            </div>";
            }

            html += $@"
        </div>
        
        <div class='footer'>
            <p>Report generated by Student Management System | {DateTime.Now:yyyy} | EduManage Platform</p>
            <p>This report contains confidential student information and should be handled according to privacy policies.</p>
        </div>
    </div>
    
    <script>
        // Auto-focus print button
        document.addEventListener('DOMContentLoaded', function() {{
            const printBtn = document.querySelector('.print-button');
            if (printBtn) {{
                printBtn.focus();
            }}
        }});
    </script>
</body>
</html>";

            return Content(html, "text/html");
        }

        // ------------------- Simple HTML Report (Fallback) ------------------- //
        private IActionResult GenerateSimpleReport(List<Student> students)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Student Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; color: #2c3e50; margin-bottom: 30px; }}
        .info {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #3498db; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .adult {{ color: #27ae60; font-weight: bold; }}
        .minor {{ color: #f39c12; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Student Management System Report</h1>
        <p>Generated on {DateTime.Now:dd MMMM yyyy HH:mm}</p>
    </div>
    
    <div class='info'>
        <strong>Report Summary:</strong><br>
        Total Students: {students.Count} | 
        Adults (18+): {students.Count(s => s.Age >= 18)} | 
        Minors (&lt;18): {students.Count(s => s.Age < 18)} | 
        Average Age: {(students.Any() ? students.Average(s => s.Age) : 0):F1} years
    </div>
    
    <table>
        <thead>
            <tr>
                <th>Student ID</th>
                <th>Name</th>
                <th>Age</th>
                <th>Category</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var student in students)
            {
                var category = student.Age >= 18 ? "Adult" : "Minor";
                var categoryClass = student.Age >= 18 ? "adult" : "minor";
                html += $@"
            <tr>
                <td>#{student.Id:D4}</td>
                <td>{student.Name ?? "N/A"}</td>
                <td>{student.Age}</td>
                <td class='{categoryClass}'>{category}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}
