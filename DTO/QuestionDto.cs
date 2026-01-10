// QuestionDto.cs
namespace OnlineExaminationSystem.DTOs
{
    // Request DTOs
    public class AddQuestionRequestDto
    {
        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; 
        public int DefaultMark { get; set; }
    }

    public class UpdateQuestionRequestDto
    {
        public int InstructorId { get; set; }
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public string? QuestionType { get; set; }
        public int? DefaultMark { get; set; }
    }

    public class AddChoiceRequestDto
    {
        public int QuestionId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class UpdateChoiceRequestDto
    {
        public int ChoiceId { get; set; }
        public string? ChoiceText { get; set; }
        public bool? IsCorrect { get; set; }
    }

    // Response DTOs
    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int DefaultMark { get; set; }
        public int CreatedByInstructorId { get; set; }
        public int ChoicesCount { get; set; }
    }

    public class ExamQuestionResponseDto
    {
        public int OrderNo { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }

    public class ChoiceDto
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class AddQuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AddChoiceResponseDto
    {
        public int ChoiceId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateResponseDto
    {
        public bool Updated { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    // Add these DTOs to your DTOs namespace
    public class AddQuestionToExamRequestDto
    {
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int OrderNo { get; set; }
        public decimal? PointsOverride { get; set; }
    }

    public class AddQuestionToExamResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int OrderNo { get; set; }
        public decimal? PointsOverride { get; set; }
    }
}
