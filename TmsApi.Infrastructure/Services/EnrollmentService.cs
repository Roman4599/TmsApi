using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.Id == id)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    }

    public async Task<EnrollmentResponseDto> EnrollStudentAsync(int courseId, int studentId, CancellationToken ct)
    {
        var exists = await _context.Enrollments
            .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId, ct);
        
        if (exists)
        {
            throw new InvalidOperationException("Student is already enrolled in this course.");
        }

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        
        var course = await _context.Courses.FindAsync(courseId, ct);
        if (course != null)
        {
            course.EnrollmentCount++;
            course.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Enrolled student {StudentId} in course {CourseId}", 
            studentId, courseId);

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId,
            enrollment.EnrolledAt
        );
    }

    public async Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _context.Enrollments
            .Where(e => e.CourseId == courseId && e.Id == id)
            .FirstOrDefaultAsync(ct);
        
        if (enrollment == null)
            return false;

        _context.Enrollments.Remove(enrollment);
        
        var course = await _context.Courses.FindAsync(courseId, ct);
        if (course != null && course.EnrollmentCount > 0)
        {
            course.EnrollmentCount--;
            course.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted enrollment {EnrollmentId} from course {CourseId}", id, courseId);
        return true;
    }
}