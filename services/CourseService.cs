using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(TmsDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.EnrollmentCount,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity,
            EnrollmentCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        return await GetByIdAsync(course.Id, ct) ?? throw new InvalidOperationException("Course not found after creation.");
    }

    public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct)
    {
        var course = await _context.Courses.FindAsync(id, ct);
        if (course == null)
            return null;

        course.Title = request.Title;
        course.MaxCapacity = request.MaxCapacity;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var course = await _context.Courses.FindAsync(id, ct);
        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted course {CourseId} ({Code})", course.Id, course.Code);
        return true;
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return await _context.Courses
            .AnyAsync(c => c.Code == code, ct);
    }
}