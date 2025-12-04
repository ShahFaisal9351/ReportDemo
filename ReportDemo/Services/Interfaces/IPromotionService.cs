using System.Collections.Generic;
using System.Threading.Tasks;
using ReportDemo.Models;
using ReportDemo.ViewModels.Promotion;

namespace ReportDemo.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<bool> CanPromoteClassAsync(int classId, string academicYear, string term);
        Task<List<Student>> GetStudentsEligibleForPromotionAsync(int classId, string academicYear);
        Task<PromotionResult> PromoteStudentsAsync(int classId, string academicYear, string promotedBy);
        Task<Class?> GetNextClassAsync(int currentClassId);
        Task<bool> IsMatricClassAsync(int classId);

        Task<PromotionViewModel> InitializePromotionViewModelAsync();
        Task<PromotionViewModel> GetStudentsForPromotionAsync(int currentSessionId, int currentClassId, int? currentSectionId = null);
        Task<(bool Success, string Message, List<PromotionHistory>? PromotedStudents)> ProcessPromotionAsync(PromotionRequestDto request);
        Task<List<PromotionHistory>> GetPromotionHistoryAsync(int? studentId = null, int? classId = null, int? sessionId = null,
            DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> IsStudentEligibleForPromotionAsync(int studentId, int currentSessionId, int currentClassId);
        Task<Class?> GetNextAvailableClassAsync(int currentClassId);
        Task<string> GenerateNewRollNumberAsync(int nextClassId, int? nextSectionId = null);
    }
}
