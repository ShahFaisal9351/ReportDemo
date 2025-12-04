using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services.Interfaces;
using ReportDemo.ViewModels.Promotion;
using System.Linq;
using System.Threading.Tasks;

namespace ReportDemo.Services
{
    public class PromotionServiceV2 : IPromotionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PromotionServiceV2> _logger;
        private readonly PromotionService _legacyPromotionService;

        public PromotionServiceV2(ApplicationDbContext context, ILogger<PromotionServiceV2> logger,
            PromotionService legacyPromotionService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _legacyPromotionService = legacyPromotionService ?? throw new ArgumentNullException(nameof(legacyPromotionService));
        }

        public async Task<PromotionViewModel> InitializePromotionViewModelAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var currentYear = currentDate.Month >= 8 ? currentDate.Year : currentDate.Year - 1;
                var currentAcademicYear = $"{currentYear}-{currentYear + 1}";

                var viewModel = new PromotionViewModel
                {
                    AvailableSessions = await _context.Sessions
                        .OrderByDescending(s => s.StartDate)
                        .ToListAsync(),
                    AvailableClasses = await _context.Classes
                        .OrderBy(c => c.Level)
                        .ThenBy(c => c.Name)
                        .ToListAsync(),
                    AvailableSections = await _context.Sections
                        .OrderBy(s => s.Name)
                        .ToListAsync(),
                    PromotionDate = DateTime.Today
                };

                // Set default next session to the next academic year
                var currentSession = viewModel.AvailableSessions
                    .FirstOrDefault(s => s.AcademicYear == currentAcademicYear);
                
                if (currentSession != null)
                {
                    var nextSession = viewModel.AvailableSessions
                        .FirstOrDefault(s => s.StartDate > currentSession.EndDate);
                    
                    if (nextSession != null)
                    {
                        viewModel.NextSessionId = nextSession.Id;
                    }
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing promotion view model");
                throw;
            }
        }

        public async Task<PromotionViewModel> GetStudentsForPromotionAsync(int currentSessionId, int currentClassId, int? currentSectionId = null)
        {
            try
            {
                var viewModel = await InitializePromotionViewModelAsync();
                viewModel.CurrentSessionId = currentSessionId;
                viewModel.CurrentClassId = currentClassId;
                viewModel.CurrentSectionId = currentSectionId;

                // Get students for the current class/section
                var query = _context.Students
                    .Include(s => s.Class)
                    .Include(s => s.Section)
                    .Where(s => s.ClassId == currentClassId);

                if (currentSectionId.HasValue)
                {
                    query = query.Where(s => s.SectionId == currentSectionId);
                }

                var students = await query.ToListAsync();

                // Get exam results for these students in the current session
                var studentIds = students.Select(s => s.Id).ToList();
                var examResults = await _context.ExamResults
                    .Where(er => studentIds.Contains(er.StudentId) && er.SessionId == currentSessionId)
                    .ToListAsync();

                // Map to view model
                viewModel.Students = students.Select(s =>
                {
                    var result = examResults.FirstOrDefault(er => er.StudentId == s.Id);
                    var isEligible = result?.IsPassed ?? false;
                    
                    return new StudentPromotionInfo
                    {
                        Id = s.Id,
                        FullName = $"{s.FirstName} {s.LastName}".Trim(),
                        RollNumber = s.RollNumber,
                        CurrentClass = s.Class?.Name ?? "N/A",
                        CurrentSection = s.Section?.Name,
                        IsEligible = isEligible,
                        HasPassed = isEligible,
                        FinalGrade = (decimal?)(result?.Percentage),
                        FinalGradeLetter = result?.Grade,
                        IsSelected = isEligible,
                        IneligibilityReason = isEligible ? null : "Failed final exams"
                    };
                }).ToList();

                // Set next class ID (default to next class in sequence)
                var nextClass = await GetNextAvailableClassAsync(currentClassId);
                if (nextClass != null)
                {
                    viewModel.NextClassId = nextClass.Id;
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students for promotion");
                throw;
            }
        }

        public async Task<(bool Success, string Message, List<PromotionHistory>? PromotedStudents)> ProcessPromotionAsync(PromotionRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var promotedStudents = new List<PromotionHistory>();

            try
            {
                // Validate request
                if (request.StudentIds == null || !request.StudentIds.Any())
                {
                    return (false, "No students selected for promotion", null);
                }

                var currentSession = await _context.Sessions.FindAsync(request.CurrentSessionId);
                var nextSession = await _context.Sessions.FindAsync(request.NextSessionId);
                var nextClass = await _context.Classes.FindAsync(request.NextClassId);

                if (currentSession == null || nextSession == null || nextClass == null)
                {
                    return (false, "Invalid session or class information", null);
                }

                // Process each student
                foreach (var studentId in request.StudentIds.Distinct())
                {
                    var student = await _context.Students
                        .Include(s => s.Class)
                        .FirstOrDefaultAsync(s => s.Id == studentId);

                    if (student == null) continue;

                    // Check if student is eligible for promotion
                    var isEligible = await IsStudentEligibleForPromotionAsync(
                        studentId, request.CurrentSessionId, request.CurrentClassId);

                    if (!isEligible)
                    {
                        _logger.LogWarning($"Student {studentId} is not eligible for promotion");
                        continue;
                    }

                    // Create promotion history record
                    var promotionHistory = new PromotionHistory
                    {
                        StudentId = student.Id,
                        OldClassId = student.ClassId,
                        NewClassId = request.NextClassId,
                        OldSessionId = request.CurrentSessionId,
                        NewSessionId = request.NextSessionId,
                        PromotionDate = request.PromotionDate,
                        AcademicYear = nextSession.AcademicYear,
                        PromotionType = "Regular",
                        PromotedBy = "System", // Should be replaced with actual user
                        Notes = request.Notes
                    };

                    // Update student record
                    student.ClassId = request.NextClassId;
                    student.SessionId = request.NextSessionId;
                    
                    if (request.NextSectionId.HasValue)
                    {
                        student.SectionId = request.NextSectionId;
                    }

                    // Generate new roll number if needed
                    if (request.GenerateNewRollNumbers)
                    {
                        student.RollNumber = await GenerateNewRollNumberAsync(
                            request.NextClassId, request.NextSectionId);
                    }

                    _context.Students.Update(student);
                    await _context.PromotionHistories.AddAsync(promotionHistory);
                    promotedStudents.Add(promotionHistory);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Successfully promoted {promotedStudents.Count} students", promotedStudents);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing student promotions");
                return (false, $"An error occurred while processing promotions: {ex.Message}", null);
            }
        }

        public async Task<List<PromotionHistory>> GetPromotionHistoryAsync(
            int? studentId = null, 
            int? classId = null, 
            int? sessionId = null,
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            try
            {
                var query = _context.PromotionHistories
                    .Include(ph => ph.Student)
                    .Include(ph => ph.OldClass)
                    .Include(ph => ph.NewClass)
                    .AsQueryable();

                if (studentId.HasValue)
                    query = query.Where(ph => ph.StudentId == studentId);

                if (classId.HasValue)
                    query = query.Where(ph => ph.OldClassId == classId || ph.NewClassId == classId);

                if (sessionId.HasValue)
                    query = query.Where(ph => ph.OldSessionId == sessionId || ph.NewSessionId == sessionId);

                if (fromDate.HasValue)
                    query = query.Where(ph => ph.PromotionDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(ph => ph.PromotionDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

                return await query
                    .OrderByDescending(ph => ph.PromotionDate)
                    .ThenBy(ph => ph.Student.LastName)
                    .ThenBy(ph => ph.Student.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving promotion history");
                throw;
            }
        }

        public async Task<bool> IsStudentEligibleForPromotionAsync(int studentId, int currentSessionId, int currentClassId)
        {
            try
            {
                // Check if student exists and is in the correct class
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == studentId && s.ClassId == currentClassId);

                if (student == null) return false;

                // Check if student is active
                if (!student.IsActive) return false;

                // Check exam results for current session
                var examResults = await _context.ExamResults
                    .Where(er => er.StudentId == studentId && er.SessionId == currentSessionId)
                    .ToListAsync();

                // Student must have at least one exam result
                if (!examResults.Any()) return false;

                // Check if student has any failed subjects
                if (examResults.Any(er => !er.IsPassed)) return false;

                // Check if student is already promoted in this session
                var existingPromotion = await _context.PromotionHistories
                    .AnyAsync(ph => ph.StudentId == studentId && 
                                  ph.OldSessionId == currentSessionId);

                return !existingPromotion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking student promotion eligibility");
                return false;
            }
        }

        public async Task<Class?> GetNextAvailableClassAsync(int currentClassId)
        {
            try
            {
                var currentClass = await _context.Classes.FindAsync(currentClassId);
                if (currentClass == null) return null;

                // Get the next class in the same stream/level
                var nextClass = await _context.Classes
                    .Where(c => c.Level > currentClass.Level)
                    .OrderBy(c => c.Level)
                    .FirstOrDefaultAsync();

                return nextClass;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next available class");
                return null;
            }
        }

        public async Task<string> GenerateNewRollNumberAsync(int nextClassId, int? nextSectionId = null)
        {
            try
            {
                var nextClass = await _context.Classes.FindAsync(nextClassId);
                if (nextClass == null)
                    throw new ArgumentException("Invalid class ID", nameof(nextClassId));

                // Get the highest roll number for the next class
                var maxRollNumber = await _context.Students
                    .Where(s => s.ClassId == nextClassId && 
                              (!nextSectionId.HasValue || s.SectionId == nextSectionId))
                    .MaxAsync(s => (int?)Convert.ToInt32(s.RollNumber)) ?? 0;

                // Format: 001, 002, etc.
                return (maxRollNumber + 1).ToString("D3");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating new roll number");
                throw;
            }
        }

        // Legacy helper wrappers
        public Task<bool> CanPromoteClassAsync(int classId, string academicYear, string term)
            => _legacyPromotionService.CanPromoteClassAsync(classId, academicYear, term);

        public Task<List<Student>> GetStudentsEligibleForPromotionAsync(int classId, string academicYear)
            => _legacyPromotionService.GetStudentsEligibleForPromotionAsync(classId, academicYear);

        public Task<PromotionResult> PromoteStudentsAsync(int classId, string academicYear, string promotedBy)
            => _legacyPromotionService.PromoteStudentsAsync(classId, academicYear, promotedBy);

        public Task<Class?> GetNextClassAsync(int currentClassId)
            => _legacyPromotionService.GetNextClassAsync(currentClassId);

        public Task<bool> IsMatricClassAsync(int classId)
            => _legacyPromotionService.IsMatricClassAsync(classId);
    }
}
