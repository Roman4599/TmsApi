using TmsApi.Entities;

public class Enrollment
{
    // Primary key
    public int Id { get; set; }

    // Foreign key to Student
    public int StudentId { get; set; }

    // Foreign key to Course
    public int CourseId { get; set; }

    // Nullable because a student may not yet have a grade
    public decimal? Grade { get; set; }

    // Date and time of enrollment
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation property to Student
    public Student Student { get; set; } = null!;

    // Navigation property to Course
    public Course Course { get; set; } = null!;
}

