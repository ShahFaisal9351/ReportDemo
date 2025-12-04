using System.ComponentModel.DataAnnotations;

namespace ReportDemo.Models
{
    public class SettingsViewModel
    {
        // Account Settings
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(500)]
        [Display(Name = "Bio")]
        public string Bio { get; set; }

        public string ProfilePicturePath { get; set; }

        // Notification Settings
        [Display(Name = "Enable Notifications")]
        public bool NotificationsEnabled { get; set; }

        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "SMS Notifications")]
        public bool SmsNotifications { get; set; }

        [Display(Name = "System Notifications")]
        public bool SystemNotifications { get; set; }

        // Appearance Settings
        [Display(Name = "Theme")]
        public string Theme { get; set; }

        [Display(Name = "Language")]
        public string Language { get; set; }

        [Display(Name = "Date Format")]
        public string DateFormat { get; set; }

        [Display(Name = "Time Format")]
        public string TimeFormat { get; set; }

        // Privacy Settings
        [Display(Name = "Profile Visibility")]
        public string ProfileVisibility { get; set; }

        [Display(Name = "Show Email")]
        public bool ShowEmail { get; set; }

        [Display(Name = "Show Phone")]
        public bool ShowPhone { get; set; }

        [Display(Name = "Allow Direct Messages")]
        public bool AllowDirectMessages { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class NotificationSettings
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool SystemNotifications { get; set; }
        public bool PromotionReminders { get; set; }
        public bool ExamNotifications { get; set; }
        public bool GradeUpdates { get; set; }
        public bool AttendanceAlerts { get; set; }
        public bool MonthlyReports { get; set; }
    }

    public class AppearanceSettings
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string TimeZone { get; set; }
        public bool CompactMode { get; set; }
        public bool ShowAnimations { get; set; }
        public bool HighContrast { get; set; }
    }

    public class PrivacySettings
    {
        public string ProfileVisibility { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowPhone { get; set; }
        public bool AllowDirectMessages { get; set; }
        public bool ShowOnlineStatus { get; set; }
        public bool AllowSearch { get; set; }
        public bool DataCollection { get; set; }
    }
}
