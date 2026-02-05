using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class AttemptAnswer
{
    public int AttemptAnswerId { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedChoiceId { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? EarnedMarks { get; set; }

    public virtual Attempt Attempt { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Choice? SelectedChoice { get; set; }
}
