namespace OnlineExaminationSystem.DTO.Instructors
{
    public class AdminAddInstructorRequestDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int BranchId { get; set; }
        public int TrackId { get; set; }
    }
}
