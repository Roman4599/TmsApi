/*Exercise 5: The Enrollment API (Controllers with Real CRUD)
Context: Now that the infrastructure is stable, you must expose HTTP endpoints for the
Angular frontend. The TMS uses MVC Controllers each resource gets its own controller
class with a constructor-injected service.
Before you code
1. Confirm builder.Services.AddControllers(); is in the builder section of Program.cs
(added in Session 2 prerequisites).
2. Confirm app.MapControllers(); is in the middleware section (after auth, before
app.Run()).
3. Create a Controllers folder in your project.*/

using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await _enrollmentService.GetByIdAsync(id);
        if (record is null)
            return NotFound();
        return Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record = await _enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}

public record CreateEnrollmentRequest(string StudentId, string CourseCode);
