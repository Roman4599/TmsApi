namespace TmsApi.Entities;

public class Certificate
{
    // Primary key
    public int Id { get; set; }

    // Human-readable certificate serial number
    public required string SerialNumber { get; set; }

    // Date certificate was issued
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to Student
    public int StudentId { get; set; }

    // Foreign key to Course
    public int CourseId { get; set; }

    // Navigation property
    public Student Student { get; set; } = null!;

    // Navigation property
    public Course Course { get; set; } = null!;
}