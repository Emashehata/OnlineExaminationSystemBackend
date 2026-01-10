namespace OnlineExaminationSystem.DTO.Instructors
{
    public class InstructorListDto
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string TrackName { get; set; } = "";
    }
}
