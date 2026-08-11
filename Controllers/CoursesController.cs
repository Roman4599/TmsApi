
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly LinkGenerator _linkGenerator;

    public CoursesController(ICourseService courseService, LinkGenerator linkGenerator)
    {
        _courseService = courseService;
        _linkGenerator = linkGenerator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(id, ct);
        if (course == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Course with ID {id} does not exist."
            });
        }

        var links = BuildCourseLinks(id, course.EnrollmentCount, course.MaxCapacity);

        var detailDto = new CourseDetailDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            course.EnrollmentCount,
            course.CreatedAt,
            course.UpdatedAt,
            links
        );

        return Ok(detailDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        if (await _courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Status = StatusCodes.Status409Conflict,
                Detail = $"A course with code '{request.Code}' is already registered."
            });
        }

        var course = await _courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
    }

    [HttpPut("{id:int}", Name = nameof(UpdateCourse))]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a course")]
    [EndpointDescription("Updates an existing course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var course = await _courseService.UpdateAsync(id, request, ct);
        if (course == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Course with ID {id} does not exist."
            });
        }

        return Ok(course);
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteCourse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes a course. Returns 204 No Content on success.")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var deleted = await _courseService.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Course with ID {id} does not exist."
            });
        }

        return NoContent();
    }

    private List<LinkDto> BuildCourseLinks(int id, int enrollmentCount, int maxCapacity)
    {
        var links = new List<LinkDto>();

        links.Add(new LinkDto(
            _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id }) ?? string.Empty,
            "self",
            "GET"
        ));

        links.Add(new LinkDto(
            _linkGenerator.GetPathByName(HttpContext, nameof(UpdateCourse), new { id }) ?? string.Empty,
            "update",
            "PUT"
        ));

        links.Add(new LinkDto(
            _linkGenerator.GetPathByName(HttpContext, nameof(DeleteCourse), new { id }) ?? string.Empty,
            "delete",
            "DELETE"
        ));

        links.Add(new LinkDto(
            _linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id }) ?? string.Empty,
            "enrollments",
            "GET"
        ));

        if (enrollmentCount < maxCapacity)
        {
            links.Add(new LinkDto(
                _linkGenerator.GetPathByName(HttpContext, "CreateEnrollment", new { courseId = id }) ?? string.Empty,
                "enroll",
                "POST"
            ));
        }

        return links;
    }
}