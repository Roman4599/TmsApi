namespace TmsApi.Domain.Entities;

public class Assessment
{
    // Primary key
    public int Id { get; set; }

    // Assessment title
    public required string Title { get; set; }

    // Maximum obtainable score
    public decimal MaxScore { get; set; }

    // Percentage contribution to the final grade
    public decimal Weight { get; set; }

    // Foreign key to Course
    public int CourseId { get; set; }

    // Navigation property
    public Course Course { get; set; } = null!;
}