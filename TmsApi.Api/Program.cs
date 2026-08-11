using TmsApi.Api.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
// ← for ApiKeyResolver

var builder = WebApplication.CreateBuilder(args);

// ================================================================
// 1. SERVICES REGISTRATION
// ================================================================

// ----- Controllers + Filters -----
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// ----- Database -----
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

// ----- Application Services -----
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();   // Exercise 3

// ----- HybridCache (Exercise 3) -----
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// ----- OpenAPI / Scalar -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// ----- RATE LIMITING (Exercise 4) -----
builder.Services.AddRateLimiter(options =>
{
    // Global limiter – applies to all endpoints (unless disabled)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"paid:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),

            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"free:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),

            _ => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"anon:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                })
        };
    });

    // Concurrency limiter for expensive endpoints (e.g., transcripts)
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;      // max 5 in‑flight
        opt.QueueLimit = 20;      // queue up to 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Optional: tighter limit for search
    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });

    // Rejection handling
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, ct);
    };
});

// ================================================================
// 2. BUILD THE APP
// ================================================================

var app = builder.Build();

// ================================================================
// 3. MIDDLEWARE PIPELINE (ORDER MATTERS!)
// ================================================================

// ----- Deprecation (Exercise 1) -----
app.UseMiddleware<V1DeprecationMiddleware>();

// ----- Exception Handling -----
app.UseExceptionHandler();          // Required for GlobalExceptionHandler
app.UseStatusCodePages();

// ----- HTTPS & Routing -----
app.UseHttpsRedirection();
app.UseRouting();

// ----- Rate Limiting (MUST be after UseRouting, before Auth) -----
app.UseRateLimiter();

// ----- Authentication / Authorization (if any) -----
// app.UseAuthentication();
// app.UseAuthorization();

// ----- Seed Data -----
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

// ----- Map Controllers -----
app.MapControllers();

// ----- OpenAPI / Scalar (Development only) -----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ----- Health Checks (exempt from rate limiting) -----
// app.MapHealthChecks("/health/live").DisableRateLimiting();
// app.MapHealthChecks("/health/ready").DisableRateLimiting();

app.Run();