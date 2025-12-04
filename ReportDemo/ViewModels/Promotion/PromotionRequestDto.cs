using System.ComponentModel.DataAnnotations;

namespace ReportDemo.ViewModels.Promotion
{
    public class PromotionRequestDto
    {
        [Required(ErrorMessage = "Current session ID is required")]
        public int CurrentSessionId { get; set; }
        
        [Required(ErrorMessage = "Current class ID is required")]
        public int CurrentClassId { get; set; }
        
        public int? CurrentSectionId { get; set; }
        
        [Required(ErrorMessage = "Next session ID is required")]
        public int NextSessionId { get; set; }
        
        [Required(ErrorMessage = "Next class ID is required")]
        public int NextClassId { get; set; }
        
        public int? NextSectionId { get; set; }
        
        [Required(ErrorMessage = "Student IDs are required")]
        public List<int> StudentIds { get; set; } = new();
        
        [Required(ErrorMessage = "Promotion date is required")]
        public DateTime PromotionDate { get; set; } = DateTime.Today;
        
        public bool GenerateNewRollNumbers { get; set; } = true;
        
        public string? Notes { get; set; }
    }
}
