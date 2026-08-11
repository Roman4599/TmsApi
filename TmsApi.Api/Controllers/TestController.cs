using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TmsDbContext _context;

    public TestController(TmsDbContext context)
    {
        _context = context;
    }

    // ================================================================
    // STEP 3: DEFERRED EXECUTION
    // ================================================================

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object...");
        var query = _context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList();

        Console.WriteLine(">>> STEP 4: Materialization finished.\n");

        return Ok(results);
    }

    // ================================================================
    // STEP 4: TRANSLATION FAILURE
    // ================================================================

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        try
        {
            var students = _context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }

    // ================================================================
    // STEP 5 - QUERY 1: Active Students Count
    // ================================================================

    [HttpGet("active-students-count")]
    public async Task<IActionResult> ActiveStudentsCount()
    {
        var count = await _context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(count);
    }

    // ================================================================
    // STEP 5 - QUERY 2: Courses by Enrollment
    // ================================================================

    [HttpGet("courses-by-enrollment")]
    public async Task<IActionResult> CoursesByEnrollment()
    {
        var list = await _context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // ================================================================
    // STEP 5 - QUERY 3: Average GPA per Course
    // ================================================================

    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // ================================================================
    // STEP 5 - QUERY 4 (Approach A): Students Without Enrollments
    // ================================================================

    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> StudentsWithoutEnrollments()
    {
        var list = await _context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // ================================================================
    // STEP 5 - QUERY 4 (Approach B - LeftJoin)
    // ================================================================

    [HttpGet("students-without-enrollments-leftjoin")]
    public async Task<IActionResult> StudentsWithoutEnrollmentsLeftJoin()
    {
        // Note: LeftJoin is a LINQ method, but EF Core may not translate it.
        // In EF Core, you can use GroupJoin + SelectMany or use raw SQL.
        // This example is kept for demonstration, but may fail at runtime.
        var list = await _context.Students
            .GroupJoin(
                _context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .SelectMany(
                x => x.e.DefaultIfEmpty(),
                (x, e) => new { x.s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
    }
}