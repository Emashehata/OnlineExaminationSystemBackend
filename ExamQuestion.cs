using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class ExamQuestion
{
    public int ExamId { get; set; }

    public int QuestionId { get; set; }

    public int OrderNo { get; set; }

    public decimal? PointsOverride { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
