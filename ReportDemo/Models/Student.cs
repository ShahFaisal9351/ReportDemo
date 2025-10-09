using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDemo.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        // Personal Information
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

        // Academic Information
        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(20, ErrorMessage = "Roll number cannot exceed 20 characters")]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Admission number is required")]
        [StringLength(30, ErrorMessage = "Admission number cannot exceed 30 characters")]
        [Display(Name = "Admission Number")]
        public string AdmissionNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required")]
        [StringLength(5, ErrorMessage = "Section cannot exceed 5 characters")]
        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Section must be a single uppercase letter (A-Z)")]
        public string Section { get; set; } = string.Empty;

        // Foreign key for Class relationship
        [Required(ErrorMessage = "Class is required")]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        // Navigation property
        public virtual Class? Class { get; set; }

        [StringLength(50, ErrorMessage = "Major subject cannot exceed 50 characters")]
        [Display(Name = "Major Subject")]
        public string? MajorSubject { get; set; }

        [Required(ErrorMessage = "Enrollment date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        // Contact Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Address Information
        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string Country { get; set; } = string.Empty;

        // Parent/Guardian Information
        [Required(ErrorMessage = "Parent/Guardian name is required")]
        [StringLength(100, ErrorMessage = "Parent/Guardian name cannot exceed 100 characters")]
        [Display(Name = "Parent/Guardian Name")]
        public string GuardianName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parent/Guardian contact is required")]
        [Phone(ErrorMessage = "Invalid parent/guardian contact format")]
        [StringLength(20, ErrorMessage = "Parent/Guardian contact cannot exceed 20 characters")]
        [Display(Name = "Parent/Guardian Contact")]
        public string GuardianContact { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Father name cannot exceed 100 characters")]
        [Display(Name = "Father Name")]
        public string? FatherName { get; set; }

        [StringLength(100, ErrorMessage = "Mother name cannot exceed 100 characters")]
        [Display(Name = "Mother Name")]
        public string? MotherName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact format")]
        [StringLength(20, ErrorMessage = "Emergency contact cannot exceed 20 characters")]
        [Display(Name = "Emergency Contact")]
        public string? EmergencyContact { get; set; }

        // Profile Information
        [StringLength(255, ErrorMessage = "Profile image path cannot exceed 255 characters")]
        [Display(Name = "Profile Image")]
        public string? ProfileImage { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        [Display(Name = "Display Name")]
        public string DisplayName => $"{FirstName} {LastName} ({RollNumber})";

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

        // Legacy property for backward compatibility
        [NotMapped]
        public string? Name => FullName;
    }
}
