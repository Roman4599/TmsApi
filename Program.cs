using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using TmsApi;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Filters; // ✅ ADDED: For the AuditLogFilter

var builder = WebApplication.CreateBuilder(args);

// === 1. SERVICES REGISTRATION ===

builder.Services.AddProblemDetails();    // Enables standardized error responses
builder.Services.AddOpenApi();           // Enables Swagger/Scalar docs

// Authentication & Authorization
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// Custom Services
builder.Services.AddSingleton<EnrollmentWorker>();
// Make sure your EnrollmentService.cs uses TmsDbContext, NOT the old Dictionary!
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Configuration Options
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// MVC Controllers (UPDATED: Registered the Global Audit Filter here)
builder.Services.AddControllers(options =>
    {
        // ✅ ADDED: The Global Action Filter (Replaces the Middleware)
        options.Filters.Add<AuditLogFilter>(); 
    })
    .ConfigureApiBehaviorOptions(options => 
    {
        options.InvalidModelStateResponseFactory = context => 
            new BadRequestObjectResult(context.ModelState);
    });

// Database Context
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    )
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

// Service Provider Validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Register Services
builder.Services.AddScoped<ICourseService, CourseService>();

var app = builder.Build();

// === 2. MIDDLEWARE PIPELINE ===

// ❌ REMOVED: app.UseMiddleware<RequestLoggingMiddleware>(); 

// Error Handling
app.UseExceptionHandler();       
app.UseStatusCodePages();        

// Standard Middleware
app.UseHttpsRedirection();
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// === 3. DEVELOPMENT TOOLS ===

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Access via /scalar/v1
}

// === 4. MINIMAL API ROUTES ===

// Existing endpoint from M4
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
}).RequireAuthorization();

// Test worker endpoint
app.MapGet("/test-worker", (EnrollmentWorker worker) =>
{
    return "worker created";
});

// Test error endpoint
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing"
    );
});

// ❌ REMOVED: app.MapGet("/test-logs", ...)  (Outdated M4 endpoint removed)

// === 5. SEED DATA (UPDATED: The new M6 Seeder) ===

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    
    // ✅ ADDED: This calls your new DataSeeder.cs (25 deterministic courses)
    await DataSeeder.SeedAsync(context);
}

app.Run();
