using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDemo.Models
{
    public class ExamResult
    {
        [Key]
        public int Id { get; set; }

        // Student Information
        [Required(ErrorMessage = "Student is required")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        // Class Information
        [Required(ErrorMessage = "Class is required")]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        // Session Information
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; } = null!;

        // Exam Details
        [Required(ErrorMessage = "Term is required")]
        [StringLength(50, ErrorMessage = "Term cannot exceed 50 characters")]
        [Display(Name = "Exam Term")]
        public string Term { get; set; } = string.Empty; // Midterm, Final, etc.

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(10, ErrorMessage = "Academic year cannot exceed 10 characters")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty; // e.g., "2024-25"

        // Results
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        [Display(Name = "Percentage")]
        public double Percentage { get; set; }

        [Required(ErrorMessage = "Grade is required")]
        [StringLength(5, ErrorMessage = "Grade cannot exceed 5 characters")]
        [Display(Name = "Grade")]
        public string Grade { get; set; } = string.Empty; // A+, A, B+, etc.

        [Display(Name = "Passed")]
        public bool IsPassed { get; set; }

        [Display(Name = "Exam Completed")]
        public bool ExamCompleted { get; set; }

        [Required(ErrorMessage = "Exam date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Exam Date")]
        public DateTime ExamDate { get; set; }

        // Additional Details
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(100, ErrorMessage = "Conducted by cannot exceed 100 characters")]
        [Display(Name = "Conducted By")]
        public string? ConductedBy { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Result Status")]
        public string ResultStatus => IsPassed ? "PASS" : "FAIL";

        [NotMapped]
        [Display(Name = "Performance Level")]
        public string PerformanceLevel
        {
            get
            {
                return Percentage switch
                {
                    >= 90 => "Excellent",
                    >= 80 => "Very Good",
                    >= 70 => "Good",
                    >= 60 => "Satisfactory",
                    >= 50 => "Average",
                    _ => "Needs Improvement"
                };
            }
        }

        [NotMapped]
        [Display(Name = "Display Name")]
        public string DisplayName => $"{Student?.FullName} - {Term} ({AcademicYear})";
    }
}