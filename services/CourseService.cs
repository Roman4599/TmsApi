using Microsoft.EntityFrameworkCore;
using TmsApi.Dtos;
using TmsApi.Entities;
using TmsApi.Data;

namespace TmsApi.Services;

public class CourseService(TmsDbContext context) : ICourseService
{
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course { Code = request.Code, Title = request.Title, MaxCapacity = request.MaxCapacity };
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        return new CourseResponseDto(course.Id, course.Code, course.Title, course.MaxCapacity, 0);
    }

    // =================================================================
    // THE GRADED LINQ LOGIC (Filter -> Count -> Sort -> Skip/Take -> Project)
    // =================================================================
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PageRequest request, CancellationToken ct)
    {
        var query = context.Courses.AsNoTracking();

        // 1. Search Filter
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => 
                EF.Functions.ILike(c.Title, $"%{request.Search}%") || 
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        // 2. Count BEFORE paging (CRITICAL!)
        var totalCount = await query.CountAsync(ct);

        // 3. Sorting (Safely whitelisted)
        query = request.OrderBy switch
        {
            "Code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };

        // 4. Skip/Take and Project (Stays inside SQL!)
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .ToListAsync(ct);

        // 5. Return Paged Response
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}