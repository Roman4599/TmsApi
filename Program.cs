using Microsoft.AspNetCore.Authentication;
using TmsApi;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;





/*var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/assessments/results", () => {
    return Results.Ok(new
{
courseCode = "CS-101",
studentId = "S-001",
letterGrade = "A"
});
}).RequireAuthorization();

app.Run();*/


// lab 4 exercise 1

/*
var builder = WebApplication.CreateBuilder(args);

// Add training authentication + authorization
builder.Services.AddAuthentication("Training").AddScheme <AuthenticationSchemeOptions, TrainingAuthHandler>
(
        "Training",
        null);
        

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})

.RequireAuthorization();

app.Run();
*/


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
// Add required services ---
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
// --- End services ---
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddControllers();
// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    )
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.MapControllers();

app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
}).RequireAuthorization();



app.MapGet("/test-worker", (EnrollmentWorker worker) =>
{
    return "worker created";
});
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing"
    );
});
app.MapGet("/test-logs", async (IEnrollmentService service) =>
{
    // Test 1: Enroll a student
    var result1 = await service.EnrollAsync("S-001", "CS-101");
    
    // Test 2: Enroll again (duplicate - should log Warning)
    var result2 = await service.EnrollAsync("S-001", "CS-101");
    
    // Test 3: Get by ID (should work)
    var result3 = await service.GetByIdAsync(result1.Id);
    
    // Test 4: Get by ID (should fail - fake ID)
    var result4 = await service.GetByIdAsync("fake123");
    
    // Test 5: Delete (should work)
    var result5 = await service.DeleteAsync(result1.Id);
    
    // Test 6: Delete again (should fail - already deleted)
    var result6 = await service.DeleteAsync(result1.Id);
    
    return Results.Ok(new { 
        message = "Check your console logs!",
        firstEnrollment = result1,
        duplicateEnrollment = result2
    });
});

using (var scope = app.Services.CreateScope())
{
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
};
context.Students.AddRange(students);
var courses = new List<Course>
{
new() { Code = "CS-101", Title = "Introduction to ComputerScience", Capacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", Capacity =
40 }
};
context.Courses.AddRange(courses);
context.SaveChanges();
var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
context.Enrollments.AddRange(enrollments);
context.SaveChanges();
}
}
app.Run();
