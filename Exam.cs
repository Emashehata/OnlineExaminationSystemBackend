using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Exam
{
    public int ExamId { get; set; }

    public int CourseId { get; set; }

    public int CreatedByInstructorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public int DurationMinutes { get; set; }

    public decimal TotalMarks { get; set; }

    public decimal? PassingScore { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Instructor CreatedByInstructor { get; set; } = null!;

    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
