using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Track
{
    public int TrackId { get; set; }

    public string TrackName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<InstructorTrack> InstructorTracks { get; set; } = new List<InstructorTrack>();

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
