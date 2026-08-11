using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(
        ICourseService courseService,
        IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrollments for a course")]
    [EndpointDescription("Returns all enrollments for a specific course.")]
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Course with ID {courseId} does not exist."
            });
        }

        var enrollments = await _enrollmentService.GetByCourseAsync(courseId, ct);
        return Ok(enrollments);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrollment for a course")]
    [EndpointDescription("Returns a specific enrollment by ID.")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        if (enrollment == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Enrollment not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Enrollment with ID {id} for course {courseId} does not exist."
            });
        }

        return Ok(enrollment);
    }

    [HttpPost(Name = "CreateEnrollment")]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Course with ID {courseId} does not exist."
            });
        }

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}."
            });
        }

        try
        {
            var enrollment = await _enrollmentService.EnrollStudentAsync(courseId, request.StudentId, ct);
            return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = enrollment.Id }, enrollment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Already enrolled",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an enrollment")]
    [EndpointDescription("Deletes an enrollment. Returns 204 No Content on success.")]
    public async Task<IActionResult> Delete(int courseId, int id, CancellationToken ct)
    {
        var deleted = await _enrollmentService.DeleteAsync(courseId, id, ct);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Enrollment not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Enrollment with ID {id} for course {courseId} does not exist."
            });
        }

        return NoContent();
    }
}