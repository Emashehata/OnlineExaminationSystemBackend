// AttemptService.cs
using Dapper;
using OnlineExaminationSystem.DTOs;
using System.Data;

namespace OnlineExaminationSystem.Services
{
    public class AttemptService : IAttemptService
    {
        public async Task<dynamic> CreateAttemptAsync(IDbConnection connection, CreateAttemptRequestDto request)
        {
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_CreateAttempt",
                new
                {
                    request.ExamId,
                    request.StudentId
                },
                commandType: CommandType.StoredProcedure
            );
            return result ?? throw new InvalidOperationException("Failed to create attempt");
        }

        public async Task<dynamic> AddStudentAnswerAsync(IDbConnection connection, AddAnswerRequestDto request)
        {
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_AddStudentAnswer",
                new
                {
                    request.AttemptId,
                    request.QuestionId,
                    request.SelectedChoiceId
                },
                commandType: CommandType.StoredProcedure
            );
            return result ?? throw new InvalidOperationException("Failed to add answer");
        }

        public async Task<dynamic> UpdateStudentAnswerAsync(IDbConnection connection, UpdateAnswerRequestDto request)
        {
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_UpdateStudentAnswer",
                new
                {
                    request.AttemptId,
                    request.QuestionId,
                    request.SelectedChoiceId
                },
                commandType: CommandType.StoredProcedure
            );
            return result ?? throw new InvalidOperationException("Failed to update answer");
        }

        public async Task<IEnumerable<AnswerResponseDto>> GetAttemptAnswersAsync(IDbConnection connection, int attemptId)
        {
            var answers = await connection.QueryAsync<AnswerResponseDto>(
                "sp_GetAttemptAnswers",
                new { AttemptId = attemptId },
                commandType: CommandType.StoredProcedure
            );
            return answers;
        }

        public async Task<IEnumerable<AttemptDto>> GetStudentAttemptsAsync(IDbConnection connection, int studentId)
        {
            var attempts = await connection.QueryAsync<AttemptDto>(
                "sp_GetStudentAttempts",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure
            );
            return attempts;
        }

        public async Task<StudentScoreDto> GetStudentScoreInExamAsync(IDbConnection connection, int studentId, int examId)
        {
            var score = await connection.QueryFirstOrDefaultAsync<StudentScoreDto>(
                "sp_GetStudentScoreInExam",
                new { StudentId = studentId, ExamId = examId },
                commandType: CommandType.StoredProcedure
            );
            return score;
        }

        public async Task<ExamSummaryDto> GetExamSummaryAsync(IDbConnection connection, int examId)
        {
            var summary = await connection.QueryFirstOrDefaultAsync<ExamSummaryDto>(
                "sp_GetExamSummary",
                new { ExamId = examId },
                commandType: CommandType.StoredProcedure
            );
            return summary;
        }

        public async Task<int> DeleteAttemptAsync(IDbConnection connection, int attemptId)
        {
            var result = await connection.ExecuteAsync(
                "sp_DeleteAttempt",
                new { AttemptId = attemptId },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<dynamic> SubmitAttemptAsync(IDbConnection connection, int attemptId)
        {
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_SubmitAttempt",
                new { AttemptId = attemptId },
                commandType: CommandType.StoredProcedure
            );
            return result ?? throw new InvalidOperationException("Failed to submit attempt");
        }
    }
}