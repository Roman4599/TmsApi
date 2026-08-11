using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Services;
using TmsApi.Filters;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// Add DbContext
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

// Register services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// Add OpenAPI and Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add HttpContextAccessor for LinkGenerator
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
