using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDemo.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(30, ErrorMessage = "Class name cannot exceed 30 characters")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Level is required")]
        [Display(Name = "Class Level")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Section is required")]
        [StringLength(5, ErrorMessage = "Section cannot exceed 5 characters")]
        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Section must be a single uppercase letter (A-Z)")]
        public string Section { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Teacher name cannot exceed 100 characters")]
        [Display(Name = "Teacher In Charge")]
        public string? TeacherInCharge { get; set; }

        [StringLength(20, ErrorMessage = "Room number cannot exceed 20 characters")]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        // Computed Properties
        [NotMapped]
        [Display(Name = "Class Display")]
        public string DisplayName => $"{ClassName} - {Section}";

        [NotMapped]
        [Display(Name = "Student Count")]
        public int StudentCount => Students?.Count ?? 0;

        [NotMapped]
        [Display(Name = "Class Name")]
        public string Name => ClassName;
    }
}