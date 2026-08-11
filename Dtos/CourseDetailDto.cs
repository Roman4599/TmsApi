namespace TmsApi.Dtos;

public record CourseDetailDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<LinkDto> Links
);