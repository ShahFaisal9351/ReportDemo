using System;
using System.ComponentModel.DataAnnotations;

namespace ReportDemo.Models
{
    public class Session
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        public bool IsCurrent { get; set; }
    }
}
