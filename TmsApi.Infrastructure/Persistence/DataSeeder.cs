using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public static class DataSeeder
{
    private static readonly (string Code, string Title, int MaxCapacity)[] Courses =
    [
        ("CSE-101", "Web Development Fundamentals", 30),
        ("CSE-102", "TypeScript Essentials", 30),
        ("CSE-103", "Git and Collaborative Workflows", 25),
        ("CSE-201", "ASP.NET Core Fundamentals", 28),
        ("CSE-202", "Entity Framework Core and PostgreSQL", 28),
        ("CSE-203", "Building RESTful Web APIs", 28),
        ("CSE-301", "Advanced Web API Patterns", 24),
        ("CSE-302", "Angular Fundamentals", 26),
        ("CSE-303", "Angular Advanced", 24),
        ("CSE-304", "Full-Stack Integration", 22),
        ("CSE-305", "Testing and Quality Assurance", 22),
        ("CSE-306", "Security and Authentication", 20),
        ("CSE-307", "DevOps and Deployment", 20),
        ("CSE-308", "Database Design and Optimization", 24),
        ("CSE-309", "Microservices Architecture", 18),
        ("CSE-310", "Cloud Computing", 20),
        ("CSE-311", "Mobile Application Development", 24),
        ("CSE-312", "UI/UX Design Principles", 26),
        ("CSE-313", "Software Project Management", 22),
        ("CSE-314", "Requirements Engineering", 22),
        ("CSE-315", "Software Testing and QA", 24),
        ("CSE-316", "Cybersecurity Fundamentals", 20),
        ("CSE-317", "Artificial Intelligence", 18),
        ("CSE-318", "Data Science", 20),
        ("CSE-319", "Machine Learning", 18)
    ];

    public static async Task SeedAsync(TmsDbContext context, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        if (await context.Courses.AnyAsync(ct))
            return;

        foreach (var (code, title, maxCapacity) in Courses)
        {
            context.Courses.Add(new Course
            {
                Code = code,
                Title = title,
                MaxCapacity = maxCapacity,
                EnrollmentCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        }

        await context.SaveChangesAsync(ct);
    }
}