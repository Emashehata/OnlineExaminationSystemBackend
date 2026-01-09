namespace OnlineExaminationSystem.DTOs
{
    public class RegisterStudentResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; } = "Student";
        public int BranchId { get; set; }
        public int TrackId { get; set; }
        public string Token { get; set; }
    }
}

