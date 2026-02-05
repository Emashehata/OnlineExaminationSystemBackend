using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public int BranchId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual User InstructorNavigation { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
