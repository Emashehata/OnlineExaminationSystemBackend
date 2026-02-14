namespace OnlineExaminationSystem.DTO.StudentsDto
{
    public class StudentSummaryDto
    {
        public int ExamsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public decimal BestScore { get; set; }
    }
    public class UpcomingDto
    {
        public int UpcomingExams { get; set; }
    }
}
