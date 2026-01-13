namespace OnlineExaminationSystem.DTO.Courses
{
    public class CourseDetailsDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string? Description { get; set; }

        public int TrackId { get; set; }
        public string TrackName { get; set; } = "";
    }
}
