using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController : ControllerBase
{
    private readonly ICachedCourseService _cachedCourseService;

    public CoursesController(ICachedCourseService cachedCourseService)
    {
        _cachedCourseService = cachedCourseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? fields,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var courses = await _cachedCourseService.GetAllCoursesAsync(ct);

        var shaped = courses.ShapeData(fields, CourseDtoFields.Allowed);

        var totalCount = courses.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var links = new List<LinkDto>
        {
            new(Url.Action(nameof(GetCourses), new { page, pageSize, fields }) ?? string.Empty, "self", "GET")
        };
        if (page < totalPages)
            links.Add(new(Url.Action(nameof(GetCourses), new { page = page + 1, pageSize, fields }) ?? string.Empty, "next", "GET"));
        if (page > 1)
            links.Add(new(Url.Action(nameof(GetCourses), new { page = page - 1, pageSize, fields }) ?? string.Empty, "prev", "GET"));

        return Ok(new
        {
            Data = shaped,
            Meta = new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            },
            Links = links
        });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetCourse(string code, CancellationToken ct)
    {
        var course = await _cachedCourseService.GetCourseAsync(code, ct);
        if (course is null) return NotFound();

        return Ok(new
        {
            Data = course,
            Links = new[]
            {
                new LinkDto(Url.Action(nameof(GetCourse), new { code }) ?? string.Empty, "self", "GET"),
                new LinkDto(Url.Action("Enroll", "Enrollments", new { courseCode = code }) ?? string.Empty, "enroll", "POST")
            }
        });
    }
}