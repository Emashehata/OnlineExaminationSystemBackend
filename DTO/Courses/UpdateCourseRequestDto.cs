namespace OnlineExaminationSystem.DTO.Courses
{
    public class UpdateCourseRequestDto
    {
        public int? TrackId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? Description { get; set; }
    }
}
