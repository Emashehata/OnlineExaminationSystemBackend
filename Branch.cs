using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Branch
{
    public int BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public virtual ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
