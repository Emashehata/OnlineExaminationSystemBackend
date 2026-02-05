using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Attempt
{
    public int AttemptId { get; set; }

    public int ExamId { get; set; }

    public int StudentId { get; set; }

    public int AttemptNo { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string Status { get; set; } = null!;

    public decimal? Score { get; set; }

    public bool? IsPassed { get; set; }

    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();

    public virtual Exam Exam { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
