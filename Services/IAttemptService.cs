// IAttemptService.cs
using OnlineExaminationSystem.DTOs;
using System.Data;

namespace OnlineExaminationSystem.Services
{
    public interface IAttemptService
    {
        Task<dynamic> CreateAttemptAsync(IDbConnection connection, CreateAttemptRequestDto request);
        Task<dynamic> AddStudentAnswerAsync(IDbConnection connection, AddAnswerRequestDto request);
        Task<dynamic> UpdateStudentAnswerAsync(IDbConnection connection, UpdateAnswerRequestDto request);
        Task<IEnumerable<AnswerResponseDto>> GetAttemptAnswersAsync(IDbConnection connection, int attemptId);
        Task<IEnumerable<AttemptDto>> GetStudentAttemptsAsync(IDbConnection connection, int studentId);
        Task<StudentScoreDto> GetStudentScoreInExamAsync(IDbConnection connection, int studentId, int examId);
        Task<ExamSummaryDto> GetExamSummaryAsync(IDbConnection connection, int examId);
        Task<int> DeleteAttemptAsync(IDbConnection connection, int attemptId);
        Task<dynamic> SubmitAttemptAsync(IDbConnection connection, int attemptId);
    }
}