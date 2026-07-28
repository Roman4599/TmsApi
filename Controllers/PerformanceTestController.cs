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

    // ===================================================================
    // EXERCISE 9: BULK ARCHIVE
    // HTTP Method: POST
    // URL: http://localhost:5076/api/perf-test/bulk-archive
    // ===================================================================
    [HttpPost("bulk-archive")]
    public async Task<IActionResult> BulkArchive(CancellationToken cancellationToken)
    {
        // Set IsArchived to true for all active enrollments in a single database-side action[cite: 2]
        int affectedRows = await _db.Enrollments
            .Where(e => !e.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true), cancellationToken);

        return Ok(new { Message = "Bulk archive complete.", RowsUpdated = affectedRows });
    }

    // ===================================================================
    // EXERCISE 9: SOFT DELETE ISOLATION DEMO
    // HTTP Method: GET
    // URL: http://localhost:5076/api/perf-test/test-soft-delete
    // ===================================================================
    [HttpGet("test-soft-delete")]
    public async Task<IActionResult> TestSoftDelete()
    {
        // Normal views automatically hide records where IsDeleted == true via HasQueryFilter[cite: 2]
        var visibleStudents = await _db.Students.CountAsync();
        var visibleCourses = await _db.Courses.CountAsync();
        var visibleEnrollments = await _db.Enrollments.CountAsync();

        // Admin view explicitly overrides query filters to see everything[cite: 2]
        var totalStudents = await _db.Students.IgnoreQueryFilters().CountAsync();
        var totalCourses = await _db.Courses.IgnoreQueryFilters().CountAsync();
        var totalEnrollments = await _db.Enrollments.IgnoreQueryFilters().CountAsync();

        return Ok(new
        {
            NormalView = new { Students = visibleStudents, Courses = visibleCourses, Enrollments = visibleEnrollments },
            AdminView = new { Students = totalStudents, Courses = totalCourses, Enrollments = totalEnrollments }
        });
    }
}