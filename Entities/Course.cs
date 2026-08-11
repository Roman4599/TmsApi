public class Course
{
    // Surrogate primary key
    public int Id { get; set; }

    // Human-readable course code
    public required string Code { get; set; }

    // Course title
    public required string Title { get; set; }

    // Maximum number of students allowed
    public int MaxCapacity { get; set; }

    // Number of enrolled students - ADD THIS
    public int EnrollmentCount { get; set; }  // ← ADD THIS

    // Creation timestamp
    public DateTime CreatedAt { get; set; }   // ← ADD THIS

    // Last updated timestamp
    public DateTime? UpdatedAt { get; set; }

    // Soft delete flag
    public bool IsDeleted { get; set; } = false;

    // Navigation property (One Course -> Many Enrollments)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}