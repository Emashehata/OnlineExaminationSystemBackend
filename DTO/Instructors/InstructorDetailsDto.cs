namespace OnlineExaminationSystem.DTO.Instructors
{
    public class InstructorDetailsDto
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public int TrackId { get; set; }
        public string TrackName { get; set; } = "";
    }
}
