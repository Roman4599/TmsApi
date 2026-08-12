using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Caching;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService : ICachedCourseService
{
    private readonly HybridCache _cache;
    private readonly TmsDbContext _context;
    private readonly ILogger<CachedCourseService> _logger;

    public CachedCourseService(
        HybridCache cache,
        TmsDbContext context,
        ILogger<CachedCourseService> logger)
    {
        _cache = cache;
        _context = context;
        _logger = logger;
    }

    public async Task<CourseDto> GetCourseAsync(string code, CancellationToken ct)
    {
        var key = CacheKeys.Course(code);
        var dbHit = false;

        var dto = await _cache.GetOrCreateAsync(
            key,
            async (token) =>
            {
                dbHit = true;
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                var course = await _context.Courses
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Code == code, token)
                    ?? throw new InvalidOperationException($"Course {code} not found.");

                return new CourseDto(
                    course.Id,
                    course.Title,
                    course.Code,
                    course.MaxCapacity,
                    course.Enrollments.Count
                );
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        // Record cache hit/miss metric
        if (dbHit)
            TmsMeters.CacheMisses.Add(1, new KeyValuePair<string, object?>("key.kind", "course"));
        else
            TmsMeters.CacheHits.Add(1, new KeyValuePair<string, object?>("key.kind", "course"));

        if (!dbHit)
            _logger.LogInformation("Cache HIT for {Key}", key);

        return dto;
    }

    public async Task<List<CourseDto>> GetAllCoursesAsync(CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;
        var dbHit = false;

        var list = await _cache.GetOrCreateAsync(
            key,
            async (token) =>
            {
                dbHit = true;
                _logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                var courses = await _context.Courses
                    .Include(c => c.Enrollments)
                    .AsNoTracking()
                    .ToListAsync(token);

                return courses.Select(c => new CourseDto(
                    c.Id,
                    c.Title,
                    c.Code,
                    c.MaxCapacity,
                    c.Enrollments.Count
                )).ToList();
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        // Record cache hit/miss metric
        if (dbHit)
            TmsMeters.CacheMisses.Add(1, new KeyValuePair<string, object?>("key.kind", "course"));
        else
            TmsMeters.CacheHits.Add(1, new KeyValuePair<string, object?>("key.kind", "course"));

        if (!dbHit)
            _logger.LogInformation("Cache HIT for {Key}", key);

        return list;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        _logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await _cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}