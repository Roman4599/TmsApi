using TmsApi.Dtos;

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);   // ← nullable
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}
