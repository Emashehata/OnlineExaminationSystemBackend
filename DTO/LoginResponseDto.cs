namespace OnlineExaminationSystem.DTOs
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int? BranchId { get; set; }
        public int? TrackId { get; set; }
        public string Token { get; set; }
    }
}
