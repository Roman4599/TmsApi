using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Entities;
using TmsApi. Data;// Adjust this namespace to match where your Student/Course/Enrollment files live

namespace TmsApi.Services
{
    public class RosterService
    {
        private readonly TmsDbContext _context;

        public RosterService(TmsDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // TODO 1: PAGINATED LIST OF STUDENTS
        // =========================================================================
        public async Task<List<Student>> GetPagedStudentsAsync(int page, CancellationToken cancellationToken)
        {
            const int pageSize = 20;

            // Simple Explanation: 
            // 1. OrderBy: Alphabetize by name first so the list order never randomly shifts.
            // 2. Skip: Mathematically calculate how many items to jump over based on the page number.
            // 3. Take: Grab exactly the next 20 items.
            // 4. ToListAsync: Send the query to PostgreSQL and handle browser cancellations gracefully.
            return await _context.Students
                .OrderBy(s => s.Name) 
                .Skip((page - 1) * pageSize) 
                .Take(pageSize) 
                .ToListAsync(cancellationToken);
        }

        // =========================================================================
        // TODO 2: TOP 5 COURSES BY ENROLLMENT COUNT
        // =========================================================================
        public async Task<List<CourseSummaryDto>> GetTopCoursesAsync()
        {
            // Simple Explanation:
            // 1. GroupBy: Cluster all individual enrollment rows together by their Course ID.
            // 2. Select: Build a clean package containing the Course Title and the total count of that cluster.
            // 3. OrderByDescending: Rank the clusters from highest count to lowest count.
            // 4. Take: Slice out and keep only the top 5 highest results.
            return await _context.Enrollments
                .GroupBy(e => new { e.CourseId, e.Course.Title })
                .Select(g => new CourseSummaryDto
                {
                    CourseTitle = g.Key.Title,
                    EnrollmentCount = g.Count() // Simple count aggregate that EF easily translates to SQL
                })
                .OrderByDescending(c => c.EnrollmentCount)
                .Take(5)
                .ToListAsync();
        }
    }

    // A small Data Transfer Object (DTO) to cleanly pass the exact summary statistics out
    public class CourseSummaryDto
    {
        public string CourseTitle { get; set; }= string.Empty;
        public int EnrollmentCount { get; set; }
    }
}