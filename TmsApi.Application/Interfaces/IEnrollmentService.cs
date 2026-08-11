

using TmsApi.Application.DTOs;
namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);   // ← nullable
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<EnrollmentResponseDto> EnrollStudentAsync(int courseId, int studentId, CancellationToken ct);
    Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct);
}