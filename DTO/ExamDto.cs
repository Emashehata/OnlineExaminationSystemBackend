// ExamDto.cs
namespace OnlineExaminationSystem.DTOs
{
    // Request DTOs
    public class CreateExamRequestDto
    {
        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingScore { get; set; }
        public bool IsPublished { get; set; } = false;
    }

    public class UpdateExamRequestDto
    {
        public int InstructorId { get; set; }
        public int ExamId { get; set; }
        public string? Title { get; set; }
        public int? DurationMinutes { get; set; }
        public int? TotalMarks { get; set; }
        public int? PassingScore { get; set; }
        public bool? IsPublished { get; set; }
    }

    public class DeleteExamRequestDto
    {
        public int InstructorId { get; set; }
        public int ExamId { get; set; }
    }

    // Response DTOs
    public class ExamResponseDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingScore { get; set; }
        public bool IsPublished { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CreatedByInstructor { get; set; } = string.Empty;
    }

    public class ExamDetailsResponseDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingScore { get; set; }
        public bool IsPublished { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CreatedByInstructor { get; set; } = string.Empty;
        public List<ExamQuestionDto> Questions { get; set; } = new();
    }

    public class ExamQuestionDto
    {
        public int OrderNo { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }


    public class ExamGradeResponseDto
    {
        public int AttemptId { get; set; }
        public int AttemptNo { get; set; }
        public decimal Score { get; set; }
        public bool IsPassed { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
    }

    public class CreateExamResponseDto
    {
        public int ExamId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateExamResponseDto
    {
        public bool Updated { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DeleteExamResponseDto
    {
        public string Result { get; set; } = string.Empty;
    }
}