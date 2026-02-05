using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class InstructorTrack
{
    public int InstructorId { get; set; }

    public int TrackId { get; set; }

    public virtual Track Track { get; set; } = null!;
}
