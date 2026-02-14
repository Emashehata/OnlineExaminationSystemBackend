namespace OnlineExaminationSystem.DTO.InstructorDashboard
{
    public class InstructorDashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int ActiveExams { get; set; }
        public decimal ClassAverage { get; set; }
        public int AtRiskStudents { get; set; }
    }
}
