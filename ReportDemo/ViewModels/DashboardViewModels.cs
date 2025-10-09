using System.ComponentModel.DataAnnotations;

namespace ReportDemo.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalTeachers { get; set; }
        public int RecentEnrollments { get; set; }
        public int EmptyClasses { get; set; }
        public double AvgStudentsPerClass { get; set; }
        
        public List<ClassStatistic> ClassStats { get; set; } = new();
        public List<RecentStudentViewModel> RecentStudents { get; set; } = new();
        public List<AgeDistribution> AgeDistribution { get; set; } = new();
        public List<GenderDistribution> GenderDistribution { get; set; } = new();
    }

    public class ClassStatistic
    {
        public string ClassName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string TeacherInCharge { get; set; } = string.Empty;
    }

    public class RecentStudentViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
        public int Age { get; set; }
    }

    public class AgeDistribution
    {
        public string AgeGroup { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class GenderDistribution
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}