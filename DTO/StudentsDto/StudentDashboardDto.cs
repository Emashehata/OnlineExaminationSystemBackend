namespace OnlineExaminationSystem.DTO.StudentsDto
{
    public class StudentDashboardDto
    {
        public int ExamsTaken { get; set; }
        public decimal AverageScore { get; set; }
        public decimal BestScore { get; set; }
        public int UpcomingExams { get; set; }

        public List<PerformanceDto> Performance { get; set; } = new();
        public List<ScoreTrendDto> ScoreTrend { get; set; } = new();
        public List<RecentResultDto> RecentResults { get; set; } = new();

        public GradeDistributionDto GradeDistribution { get; set; }
    }
}
