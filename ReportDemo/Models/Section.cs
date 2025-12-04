using System.ComponentModel.DataAnnotations;

namespace ReportDemo.Models
{
    public class Section
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public int? ClassId { get; set; }
        public virtual Class? Class { get; set; }
    }
}
