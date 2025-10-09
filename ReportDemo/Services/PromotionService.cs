using ReportDemo.Data;
using ReportDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace ReportDemo.Services
{
    public interface IPromotionService
    {
        Task<bool> CanPromoteClassAsync(int classId, string academicYear, string term);
        Task<List<Student>> GetStudentsEligibleForPromotionAsync(int classId, string academicYear);
        Task<PromotionResult> PromoteStudentsAsync(int classId, string academicYear, string promotedBy);
        Task<Class?> GetNextClassAsync(int currentClassId);
        Task<bool> IsMatricClassAsync(int classId);
    }

    public class PromotionResult
    {
        public int TotalStudents { get; set; }
        public int PromotedCount { get; set; }
        public int GraduatedCount { get; set; }
        public int RetainedCount { get; set; }
        public List<string> Messages { get; set; } = new();
        public bool Success { get; set; }
    }

    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanPromoteClassAsync(int classId, string academicYear, string term)
        {
            // Check if all students in the class have completed exams
            var studentsInClass = await _context.Students
                .Where(s => s.ClassId == classId)
                .Select(s => s.Id)
                .ToListAsync();

            if (!studentsInClass.Any())
                return false;

            // Check if all students have exam results for the specified term and academic year
            var studentsWithResults = await _context.ExamResults
                .Where(er => studentsInClass.Contains(er.StudentId) && 
                           er.AcademicYear == academicYear && 
                           er.Term == term &&
                           er.ExamCompleted)
                .Select(er => er.StudentId)
                .Distinct()
                .CountAsync();

            return studentsWithResults == studentsInClass.Count;
        }

        public async Task<List<Student>> GetStudentsEligibleForPromotionAsync(int classId, string academicYear)
        {
            return await _context.Students
                .Include(s => s.Class)
                .Where(s => s.ClassId == classId)
                .ToListAsync();
        }

        public async Task<PromotionResult> PromoteStudentsAsync(int classId, string academicYear, string promotedBy)
        {
            var result = new PromotionResult();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var currentClass = await _context.Classes.FindAsync(classId);
                if (currentClass == null)
                {
                    result.Messages.Add("Class not found");
                    return result;
                }

                var students = await GetStudentsEligibleForPromotionAsync(classId, academicYear);
                result.TotalStudents = students.Count;

                var nextClass = await GetNextClassAsync(classId);
                var isMatricClass = await IsMatricClassAsync(classId);

                foreach (var student in students)
                {
                    // Get student's final exam result
                    var finalResult = await _context.ExamResults
                        .Where(er => er.StudentId == student.Id && 
                                   er.AcademicYear == academicYear)
                        .OrderByDescending(er => er.ExamDate)
                        .FirstOrDefaultAsync();

                    var isPassed = finalResult?.IsPassed ?? false;
                    var finalPercentage = finalResult?.Percentage ?? 0;
                    var finalGrade = finalResult?.Grade ?? "N/A";

                    if (isMatricClass)
                    {
                        // Graduate the student
                        await GraduateStudentAsync(student, currentClass, academicYear, finalPercentage, finalGrade, promotedBy);
                        result.GraduatedCount++;
                    }
                    else if (isPassed && nextClass != null)
                    {
                        // Promote to next class
                        await PromoteStudentAsync(student, currentClass, nextClass, academicYear, finalPercentage, finalGrade, promotedBy);
                        result.PromotedCount++;
                    }
                    else
                    {
                        // Retain in same class
                        await RetainStudentAsync(student, currentClass, academicYear, finalPercentage, finalGrade, promotedBy);
                        result.RetainedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                result.Messages.Add($"Promotion completed: {result.PromotedCount} promoted, {result.GraduatedCount} graduated, {result.RetainedCount} retained");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Messages.Add($"Error during promotion: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        private async Task PromoteStudentAsync(Student student, Class oldClass, Class newClass, string academicYear, 
            double finalPercentage, string finalGrade, string promotedBy)
        {
            // Update student's class
            student.ClassId = newClass.Id;
            student.UpdatedAt = DateTime.UtcNow;

            // Create promotion history record
            var promotionHistory = new PromotionHistory
            {
                StudentId = student.Id,
                OldClassId = oldClass.Id,
                NewClassId = newClass.Id,
                PromotionDate = DateTime.UtcNow,
                AcademicYear = academicYear,
                PromotionType = "Regular",
                PromotedBy = promotedBy,
                FinalPercentage = finalPercentage,
                FinalGrade = finalGrade,
                IsPromoted = true,
                IsGraduated = false
            };

            _context.PromotionHistories.Add(promotionHistory);
        }

        private async Task GraduateStudentAsync(Student student, Class graduationClass, string academicYear, 
            double finalPercentage, string finalGrade, string promotedBy)
        {
            // Create alumni record
            var alumni = new Alumni
            {
                OriginalStudentId = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Gender = student.Gender,
                DateOfBirth = student.DateOfBirth,
                RollNumber = student.RollNumber,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                City = student.City,
                Country = student.Country,
                GuardianName = student.GuardianName,
                GuardianContact = student.GuardianContact,
                ProfileImage = student.ProfileImage,
                GraduatedFromClassId = graduationClass.Id,
                GraduationDate = DateTime.UtcNow,
                AcademicYear = academicYear,
                FinalPercentage = finalPercentage,
                FinalGrade = finalGrade,
                GraduationStatus = GetGraduationStatus(finalPercentage),
                EnrollmentDate = student.EnrollmentDate
            };

            _context.Alumni.Add(alumni);

            // Create promotion history record
            var promotionHistory = new PromotionHistory
            {
                StudentId = student.Id,
                OldClassId = graduationClass.Id,
                NewClassId = null,
                PromotionDate = DateTime.UtcNow,
                AcademicYear = academicYear,
                PromotionType = "Graduated",
                PromotedBy = promotedBy,
                FinalPercentage = finalPercentage,
                FinalGrade = finalGrade,
                IsPromoted = false,
                IsGraduated = true
            };

            _context.PromotionHistories.Add(promotionHistory);

            // Remove student from active students
            _context.Students.Remove(student);
        }

        private async Task RetainStudentAsync(Student student, Class currentClass, string academicYear, 
            double finalPercentage, string finalGrade, string promotedBy)
        {
            // Create promotion history record for retention
            var promotionHistory = new PromotionHistory
            {
                StudentId = student.Id,
                OldClassId = currentClass.Id,
                NewClassId = currentClass.Id,
                PromotionDate = DateTime.UtcNow,
                AcademicYear = academicYear,
                PromotionType = "Retained",
                PromotedBy = promotedBy,
                FinalPercentage = finalPercentage,
                FinalGrade = finalGrade,
                IsPromoted = false,
                IsGraduated = false,
                Remarks = "Student retained in same class"
            };

            _context.PromotionHistories.Add(promotionHistory);
        }

        public async Task<Class?> GetNextClassAsync(int currentClassId)
        {
            var currentClass = await _context.Classes.FindAsync(currentClassId);
            if (currentClass == null) return null;

            var nextClassName = GetNextClassName(currentClass.ClassName);
            if (nextClassName == null) return null;

            return await _context.Classes
                .Where(c => c.ClassName == nextClassName)
                .OrderBy(c => c.Section)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsMatricClassAsync(int classId)
        {
            var classInfo = await _context.Classes.FindAsync(classId);
            return classInfo?.ClassName == "10";
        }

        private string? GetNextClassName(string currentClassName)
        {
            return currentClassName.ToUpper() switch
            {
                "NURSERY" => "KG",
                "KG" => "1",
                "1" => "2",
                "2" => "3",
                "3" => "4",
                "4" => "5",
                "5" => "6",
                "6" => "7",
                "7" => "8",
                "8" => "9",
                "9" => "10",
                "10" => null, // Matric - no next class
                _ => null
            };
        }

        private string GetGraduationStatus(double percentage)
        {
            return percentage switch
            {
                >= 90 => "Honor",
                >= 75 => "Merit",
                _ => "Regular"
            };
        }
    }
}