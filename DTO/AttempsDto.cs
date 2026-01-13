// AttemptsDto.cs
namespace OnlineExaminationSystem.DTOs
{
    // Request DTOs
    public class CreateAttemptRequestDto
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }
    }

    public class AddAnswerRequestDto
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedChoiceId { get; set; }
    }

    public class UpdateAnswerRequestDto
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedChoiceId { get; set; }
    }

    public class DeleteAttemptRequestDto
    {
        public int AttemptId { get; set; }
    }

    // Response DTOs
    public class CreateAttemptResponseDto
    {
        public decimal AttemptId { get; set; }
        public int AttemptNo { get; set; }
        public DateTime StartedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AnswerResponseDto
    {
        public int AttemptAnswerId { get; set; }
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int SelectedChoiceId { get; set; }
        public string SelectedChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public decimal EarnedMarks { get; set; }
    }

    public class AttemptDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int AttemptNo { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal? Score { get; set; }
        public bool? IsPassed { get; set; }
    }

    public class StudentScoreDto
    {
        public int AttemptId { get; set; }
        public int AttemptNo { get; set; }
        public decimal? Score { get; set; }
        public bool? IsPassed { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AttemptsCount { get; set; }
        public decimal AvgScore { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal PassRatePercent { get; set; }
    }

    public class SaveAnswerResponseDto
    {
        public bool Saved { get; set; }
        public bool IsCorrect { get; set; }
        public decimal EarnedMarks { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DeleteResponseDto
    {
        public bool Deleted { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class CreateAttemptResult
    {
        public int AttemptId { get; set; }
        public int AttemptNo { get; set; }
    }

    public class SaveAnswerResult
    {
        public int Saved { get; set; }
        public bool IsCorrect { get; set; }
        public decimal EarnedMarks { get; set; }
    }

    public class UpdateAnswerResult
    {
        public int Updated { get; set; }
        public bool IsCorrect { get; set; }
        public decimal EarnedMarks { get; set; }
    }
}