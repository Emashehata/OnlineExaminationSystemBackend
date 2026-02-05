using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByAdminId { get; set; }

    public virtual User? CreatedByAdmin { get; set; }

    public virtual Instructor? Instructor { get; set; }

    public virtual ICollection<User> InverseCreatedByAdmin { get; set; } = new List<User>();

    public virtual Role Role { get; set; } = null!;

    public virtual Student? Student { get; set; }
}
