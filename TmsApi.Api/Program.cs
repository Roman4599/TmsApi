using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseMiddleware<V1DeprecationMiddleware>();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public class V1DeprecationMiddleware
{
    private readonly RequestDelegate _next;

    public V1DeprecationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/v1", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers["Deprecation"] = "true";
            context.Response.Headers["Sunset"] = DateTime.UtcNow.AddMonths(1).ToString("R");
        }

        await _next(context);
    }
}