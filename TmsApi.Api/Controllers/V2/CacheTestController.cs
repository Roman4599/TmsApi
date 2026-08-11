using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/cache-test")]
[ApiVersion("2.0")]
public class CacheTestController : ControllerBase
{
    private readonly ICachedCourseService _cachedCourseService;

    public CacheTestController(ICachedCourseService cachedCourseService)
    {
        _cachedCourseService = cachedCourseService;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetAllCourses(CancellationToken ct)
    {
        var courses = await _cachedCourseService.GetAllCoursesAsync(ct);
        return Ok(new
        {
            count = courses.Count,
            courses = courses.Take(5)
        });
    }

    [HttpGet("course/{code}")]
    public async Task<IActionResult> GetCourse(string code, CancellationToken ct)
    {
        try
        {
            var course = await _cachedCourseService.GetCourseAsync(code, ct);
            return Ok(course);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = $"Course '{code}' not found." });
        }
    }

    [HttpPost("invalidate")]
    public async Task<IActionResult> InvalidateCache(CancellationToken ct)
    {
        await _cachedCourseService.InvalidateCourseCacheAsync(ct);
        return Ok(new { message = "Cache invalidated" });
    }
}