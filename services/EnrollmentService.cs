using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;   // ← Must be Services, not Controllers

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto> EnrollStudentAsync(int courseId, int studentId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    // ... all methods (GetByIdAsync, GetByCourseAsync, EnrollStudentAsync, DeleteAsync)
}