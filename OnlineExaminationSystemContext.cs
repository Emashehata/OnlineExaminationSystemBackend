using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OnlineExaminationSystem;

public partial class OnlineExaminationSystemContext : DbContext
{
    public OnlineExaminationSystemContext()
    {
    }

    public OnlineExaminationSystemContext(DbContextOptions<OnlineExaminationSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<AttemptAnswer> AttemptAnswers { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<InstructorTrack> InstructorTracks { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection String 'DefaultConnection' not found");

        optionsBuilder.UseSqlServer(connectionString);
    }

  


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.HasIndex(e => e.ExamId, "IX_Attempts_ExamId");

            entity.HasIndex(e => e.StudentId, "IX_Attempts_StudentId");

            entity.HasIndex(e => new { e.ExamId, e.StudentId, e.AttemptNo }, "UQ_Attempts_Exam_Student_Attempt").IsUnique();

            entity.Property(e => e.Score).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("InProgress");

            entity.HasOne(d => d.Exam).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attempts_Exams");

            entity.HasOne(d => d.Student).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attempts_Students");
        });

        modelBuilder.Entity<AttemptAnswer>(entity =>
        {
            entity.HasIndex(e => e.AttemptId, "IX_AttemptAnswers_AttemptId");

            entity.HasIndex(e => new { e.AttemptId, e.QuestionId }, "UQ_AttemptAnswers_Attempt_Question").IsUnique();

            entity.Property(e => e.EarnedMarks).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Attempt).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttemptAnswers_Attempts");

            entity.HasOne(d => d.Question).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttemptAnswers_Questions");

            entity.HasOne(d => d.SelectedChoice).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.SelectedChoiceId)
                .HasConstraintName("FK_AttemptAnswers_Choices");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(e => e.BranchName, "UQ_Branches_BranchName").IsUnique();

            entity.Property(e => e.BranchName).HasMaxLength(100);
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Choices_ValidateCounts"));

            entity.HasIndex(e => e.QuestionId, "IX_Choices_QuestionId");

            entity.Property(e => e.ChoiceText).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.Choices)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Choices_Questions");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK_Courses_1");

            entity.HasIndex(e => e.CourseCode, "UQ_Courses_Code").IsUnique();

            entity.Property(e => e.CourseCode).HasMaxLength(30);
            entity.Property(e => e.CourseName).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(d => d.Tracks).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseTrack",
                    r => r.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CourseTra__Track__65370702"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CourseTra__Cours__6442E2C9"),
                    j =>
                    {
                        j.HasKey("CourseId", "TrackId").HasName("PK__CourseTr__DE8A3E2921FD7B46");
                        j.ToTable("CourseTrack");
                    });
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Exams_CourseId");

            entity.HasIndex(e => e.CreatedByInstructorId, "IX_Exams_InstructorId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PassingScore).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TotalMarks).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CreatedByInstructor).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CreatedByInstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Instructors");
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => new { e.ExamId, e.QuestionId });

            entity.HasIndex(e => e.ExamId, "IX_ExamQuestions_ExamId");

            entity.HasIndex(e => new { e.ExamId, e.OrderNo }, "UQ_ExamQuestions_Order").IsUnique();

            entity.Property(e => e.PointsOverride).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamQuestions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamQuestions_Exams");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamQuestions_Questions");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Instructors_RoleCheck"));

            entity.Property(e => e.InstructorId).ValueGeneratedNever();

            entity.HasOne(d => d.Branch).WithMany(p => p.Instructors)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Instructors_Branches");

            entity.HasOne(d => d.InstructorNavigation).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Instructors_Users");
        });

        modelBuilder.Entity<InstructorTrack>(entity =>
        {
            entity.HasKey(e => new { e.InstructorId, e.TrackId }).HasName("PK__Instruct__8AA645156963B806");

            entity.ToTable("InstructorTrack");

            entity.HasOne(d => d.Track).WithMany(p => p.InstructorTracks)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__Track__70A8B9AE");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Questions_CourseId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DefaultMark)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.QuestionText).HasMaxLength(2000);
            entity.Property(e => e.QuestionType).HasMaxLength(10);

            entity.HasOne(d => d.CreatedByInstructor).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CreatedByInstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Questions_Instructors");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "UQ_Roles_RoleName").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(20);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Students_RoleCheck"));

            entity.Property(e => e.StudentId).ValueGeneratedNever();

            entity.HasOne(d => d.Branch).WithMany(p => p.Students)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Branches");

            entity.HasOne(d => d.StudentNavigation).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__Topics__022E0F5DACA170FF");

            entity.HasIndex(e => new { e.CourseId, e.TopicName }, "UQ_Topics_Course_TopicName").IsUnique();

            entity.Property(e => e.TopicName).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.Topics)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topics_Courses");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.TrackId).HasName("PK_Tracks_1");

            entity.HasIndex(e => e.TrackName, "UQ_TrackName").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TrackName).HasMaxLength(120);

            entity.HasMany(d => d.Branches).WithMany(p => p.Tracks)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackBranch",
                    r => r.HasOne<Branch>().WithMany()
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TrackBran__Branc__6166761E"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TrackBran__Track__607251E5"),
                    j =>
                    {
                        j.HasKey("TrackId", "BranchId").HasName("PK__TrackBra__30627A1CE264D9DC");
                        j.ToTable("TrackBranch");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Users_AdminCreatesInstructorOnly"));

            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByAdmin).WithMany(p => p.InverseCreatedByAdmin)
                .HasForeignKey(d => d.CreatedByAdminId)
                .HasConstraintName("Fk_Users_CreatedByAdmin");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
