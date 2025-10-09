using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDemo.Models
{
    public class Alumni
    {
        [Key]
        public int Id { get; set; }

        // Student Information (copied from Student at graduation)
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(20, ErrorMessage = "Roll number cannot exceed 20 characters")]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string? Country { get; set; }

        [StringLength(100, ErrorMessage = "Guardian name cannot exceed 100 characters")]
        [Display(Name = "Guardian Name")]
        public string? GuardianName { get; set; }

        [StringLength(20, ErrorMessage = "Guardian contact cannot exceed 20 characters")]
        [Display(Name = "Guardian Contact")]
        public string? GuardianContact { get; set; }

        [StringLength(255, ErrorMessage = "Profile image path cannot exceed 255 characters")]
        [Display(Name = "Profile Image")]
        public string? ProfileImage { get; set; }

        // Graduation Information
        [Required(ErrorMessage = "Original student ID is required")]
        [Display(Name = "Original Student ID")]
        public int OriginalStudentId { get; set; }

        [Required(ErrorMessage = "Graduated from class is required")]
        [Display(Name = "Graduated From Class")]
        public int GraduatedFromClassId { get; set; }

        [ForeignKey("GraduatedFromClassId")]
        public virtual Class GraduatedFromClass { get; set; } = null!;

        [Required(ErrorMessage = "Graduation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Graduation Date")]
        public DateTime GraduationDate { get; set; }

        [Required(ErrorMessage = "Academic year is required")]
        [StringLength(10, ErrorMessage = "Academic year cannot exceed 10 characters")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Final percentage must be between 0 and 100")]
        [Display(Name = "Final Percentage")]
        public double? FinalPercentage { get; set; }

        [StringLength(5, ErrorMessage = "Final grade cannot exceed 5 characters")]
        [Display(Name = "Final Grade")]
        public string? FinalGrade { get; set; }

        [StringLength(20, ErrorMessage = "Graduation status cannot exceed 20 characters")]
        [Display(Name = "Graduation Status")]
        public string GraduationStatus { get; set; } = string.Empty; // "Regular", "Honor", "Merit"

        // Post-Graduation Information
        [StringLength(100, ErrorMessage = "Current occupation cannot exceed 100 characters")]
        [Display(Name = "Current Occupation")]
        public string? CurrentOccupation { get; set; }

        [StringLength(100, ErrorMessage = "Current employer cannot exceed 100 characters")]
        [Display(Name = "Current Employer")]
        public string? CurrentEmployer { get; set; }

        [StringLength(100, ErrorMessage = "Higher education cannot exceed 100 characters")]
        [Display(Name = "Higher Education")]
        public string? HigherEducation { get; set; }

        [StringLength(100, ErrorMessage = "Current email cannot exceed 100 characters")]
        [Display(Name = "Current Email")]
        [EmailAddress(ErrorMessage = "Invalid current email format")]
        public string? CurrentEmail { get; set; }

        [StringLength(20, ErrorMessage = "Current phone cannot exceed 20 characters")]
        [Display(Name = "Current Phone")]
        public string? CurrentPhone { get; set; }

        [StringLength(200, ErrorMessage = "Current address cannot exceed 200 characters")]
        [Display(Name = "Current Address")]
        public string? CurrentAddress { get; set; }

        // Timestamps
        [Display(Name = "Enrollment Date")]
        public DateTime? EnrollmentDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        [Display(Name = "Age")]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        [Display(Name = "Years Since Graduation")]
        public int YearsSinceGraduation
        {
            get
            {
                var today = DateTime.Today;
                var years = today.Year - GraduationDate.Year;
                if (GraduationDate.Date > today.AddYears(-years)) years--;
                return years;
            }
        }

        [NotMapped]
        [Display(Name = "Display Name")]
        public string DisplayName => $"{FullName} ({RollNumber})";
    }
}