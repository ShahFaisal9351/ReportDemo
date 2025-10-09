using ReportDemo.Data;
using ReportDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace ReportDemo.Services
{
    public interface IRollNumberService
    {
        Task<string> GenerateRollNumberAsync(int classId);
        Task<bool> IsRollNumberUniqueAsync(string rollNumber, int? excludeStudentId = null);
    }

    public class RollNumberService : IRollNumberService
    {
        private readonly ApplicationDbContext _context;

        public RollNumberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateRollNumberAsync(int classId)
        {
            var classInfo = await _context.Classes.FindAsync(classId);
            if (classInfo == null)
                throw new ArgumentException("Invalid class ID", nameof(classId));

            var currentYear = DateTime.Now.Year;
            var classCode = GetClassCode(classInfo.ClassName);
            
            // Get the next sequential number for this class and year
            var prefix = $"{classCode}-{currentYear}-";
            var existingRollNumbers = await _context.Students
                .Where(s => s.RollNumber.StartsWith(prefix))
                .Select(s => s.RollNumber)
                .ToListAsync();

            var maxNumber = 0;
            foreach (var rollNumber in existingRollNumbers)
            {
                var numberPart = rollNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            var nextNumber = maxNumber + 1;
            return $"{prefix}{nextNumber:D3}";
        }

        public async Task<bool> IsRollNumberUniqueAsync(string rollNumber, int? excludeStudentId = null)
        {
            var query = _context.Students.Where(s => s.RollNumber == rollNumber);
            
            if (excludeStudentId.HasValue)
            {
                query = query.Where(s => s.Id != excludeStudentId.Value);
            }

            return !await query.AnyAsync();
        }

        private static string GetClassCode(string className)
        {
            return className.ToUpper() switch
            {
                "NURSERY" => "NUR",
                "KG" => "KG",
                "1" => "CLS1",
                "2" => "CLS2",
                "3" => "CLS3",
                "4" => "CLS4",
                "5" => "CLS5",
                "6" => "CLS6",
                "7" => "CLS7",
                "8" => "CLS8",
                "9" => "CLS9",
                "10" => "CLS10",
                _ => "CLS"
            };
        }
    }
}