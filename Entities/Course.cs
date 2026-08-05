public class Course
{
    // Surrogate primary key
    public int Id { get; set; }

    // Human-readable course code
    public required string Code { get; set; }

    // Course title
    public required string Title { get; set; }

    // Maximum number of students allowed (RENAMED from Capacity)
    public int MaxCapacity { get; set; }  // <-- THIS IS THE CHANGE

    // Navigation property (One Course -> Many Enrollments)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public bool IsDeleted { get; set; } = false;
}