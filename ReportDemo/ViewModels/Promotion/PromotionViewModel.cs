using System.ComponentModel.DataAnnotations;
using ReportDemo.Models;

namespace ReportDemo.ViewModels.Promotion
{
    public class PromotionViewModel
    {
        // Current Academic Information
        [Display(Name = "Current Session")]
        [Required(ErrorMessage = "Current session is required")]
        public int CurrentSessionId { get; set; }
        public List<Session>? AvailableSessions { get; set; }

        [Display(Name = "Current Class")]
        [Required(ErrorMessage = "Current class is required")]
        public int CurrentClassId { get; set; }
        public List<Class>? AvailableClasses { get; set; }

        [Display(Name = "Current Section")]
        public int? CurrentSectionId { get; set; }
        public List<Section>? AvailableSections { get; set; }

        // Promotion Target
        [Display(Name = "Next Session")]
        [Required(ErrorMessage = "Next session is required")]
        public int NextSessionId { get; set; }

        [Display(Name = "Next Class")]
        [Required(ErrorMessage = "Next class is required")]
        public int NextClassId { get; set; }
        
        [Display(Name = "Next Section")]
        public int? NextSectionId { get; set; }

        // Student List
        public List<StudentPromotionInfo>? Students { get; set; }
        
        // Promotion Options
        [Display(Name = "Promotion Date")]
        [DataType(DataType.Date)]
        public DateTime PromotionDate { get; set; } = DateTime.Today;
        
        [Display(Name = "Generate New Roll Numbers")]
        public bool GenerateNewRollNumbers { get; set; } = true;
        
        [Display(Name = "Skip Failed Students")]
        public bool SkipFailedStudents { get; set; } = true;
        
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class StudentPromotionInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string CurrentClass { get; set; } = string.Empty;
        public string? CurrentSection { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public bool IsSelected { get; set; }
        public string? NewRollNumber { get; set; }
        public bool HasPassed { get; set; } = true;
        public decimal? FinalGrade { get; set; }
        public string? FinalGradeLetter { get; set; }
    }
}
