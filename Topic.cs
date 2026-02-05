using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class Topic
{
    public int TopicId { get; set; }

    public int CourseId { get; set; }

    public string TopicName { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
}
