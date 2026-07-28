using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/perf-test")]
public class PerformanceTestController : ControllerBase
{
    private readonly TmsDbContext _db;

    public PerformanceTestController(TmsDbContext db)
    {
        _db = db;
    }

    [HttpGet("run")]
    public async Task<IActionResult> RunTest(CancellationToken cancellationToken)
    {
        // ===================================================================
        // STEP 1: RUN PART A (THE BUG)
        // Run this first, save, and hit: http://localhost:5076/api/perf-test/run
        // Look at your VS Code terminal window. You will see 1 + N SQL queries!
        // ===================================================================
        var students = await _db.Students.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var s in students)
        {
            var count = await _db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);
            
            Console.WriteLine($"[PART A LOG] {s.Name}: {count} enrollments");
        }

        // ===================================================================
        // STEP 2: RUN PART B (THE FIX)
        // After you see the bug, comment out Part A, uncomment Part B below, 
        // save, and hit the URL again. You will see exactly ONE single SQL query!
        // ===================================================================
        /*
        var report = await _db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var r in report)
        {
            Console.WriteLine($"[PART B LOG] {r.Name}: {r.EnrollmentCount} enrollments");
        }
        */

        return Ok("Test executed! Check your VS Code terminal window to count the SQL queries.");
    }
}
