using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Student
{
    public int StudentId { get; set; }

    public int BranchId { get; set; }

    public int TrackId { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual User StudentNavigation { get; set; } = null!;
}
