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
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddControllers();
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


app.Run();
