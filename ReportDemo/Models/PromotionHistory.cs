using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDemo.Models
{
    public class PromotionHistory
    {
        [Key]
        public int Id { get; set; }

        // Student Information
        [Required(ErrorMessage = "Student is required")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        // Class Information
        [Required(ErrorMessage = "Old class is required")]
        [Display(Name = "From Class")]
        public int OldClassId { get; set; }

        [ForeignKey("OldClassId")]
        public virtual Class OldClass { get; set; } = null!;

        [Display(Name = "To Class")]
        public int? NewClassId { get; set; } // Null if graduated

        [ForeignKey("NewClassId")]
        public virtual Class? NewClass { get; set; }

        // Session Information
        [Required]
        public int OldSessionId { get; set; }

        [ForeignKey("OldSessionId")]
        public virtual Session OldSession { get; set; } = null!;

        public int? NewSessionId { get; set; }

        [ForeignKey("NewSessionId")]
        public virtual Session? NewSession { get; set; }

        // Promotion Details
        [Required(ErrorMessage = "Promotion date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Promotion Date")]
        public DateTime PromotionDate { get; set; }

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(10, ErrorMessage = "Academic year cannot exceed 10 characters")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty; // e.g., "2024-25"

        [Required(ErrorMessage = "Promotion type is required")]
        [StringLength(20, ErrorMessage = "Promotion type cannot exceed 20 characters")]
        [Display(Name = "Promotion Type")]
        public string PromotionType { get; set; } = string.Empty; // "Regular", "Merit", "Graduated"

        [StringLength(100, ErrorMessage = "Promoted by cannot exceed 100 characters")]
        [Display(Name = "Promoted By")]
        public string? PromotedBy { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Performance Data
        [Range(0, 100, ErrorMessage = "Final percentage must be between 0 and 100")]
        [Display(Name = "Final Percentage")]
        public double? FinalPercentage { get; set; }

        [StringLength(5, ErrorMessage = "Final grade cannot exceed 5 characters")]
        [Display(Name = "Final Grade")]
        public string? FinalGrade { get; set; }

        [Display(Name = "Promoted")]
        public bool IsPromoted { get; set; }

        [Display(Name = "Graduated")]
        public bool IsGraduated { get; set; }

        // Additional Information
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Promotion Status")]
        public string PromotionStatus
        {
            get
            {
                if (IsGraduated) return "GRADUATED";
                if (IsPromoted) return "PROMOTED";
                return "RETAINED";
            }
        }

        [NotMapped]
        [Display(Name = "Display Name")]
        public string DisplayName
        {
            get
            {
                if (IsGraduated)
                    return $"{Student?.FullName}: {OldClass?.DisplayName} → GRADUATED";
                if (NewClass != null)
                    return $"{Student?.FullName}: {OldClass?.DisplayName} → {NewClass.DisplayName}";
                return $"{Student?.FullName}: {OldClass?.DisplayName} → RETAINED";
            }
        }

        [NotMapped]
        [Display(Name = "Class Transition")]
        public string ClassTransition
        {
            get
            {
                if (IsGraduated) return $"{OldClass?.DisplayName} → GRADUATED";
                if (NewClass != null) return $"{OldClass?.DisplayName} → {NewClass.DisplayName}";
                return $"{OldClass?.DisplayName} → RETAINED";
            }
        }
    }
}