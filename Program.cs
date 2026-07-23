

using Microsoft.AspNetCore.Authentication;




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
// --- Add required services ---
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
// --- End services ---
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

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
}).RequireAuthorization();



app.MapGet("/test-worker", (EnrollmentWorker worker) =>
{
    return "worker created";
});

app.Run();
