using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Question
{
    public int QuestionId { get; set; }

    public int CourseId { get; set; }

    public int CreatedByInstructorId { get; set; }

    public string QuestionType { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public decimal DefaultMark { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

    public virtual Instructor CreatedByInstructor { get; set; } = null!;

    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
