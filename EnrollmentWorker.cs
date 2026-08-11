using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TmsApi.Services;   // ← ADDED

namespace TmsApi.Workers;   // adjust namespace if needed

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnrollmentWorker> _logger;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        // Your logic here...
        _logger.LogInformation("Processing enrollment batch...");
        await Task.CompletedTask;
    }
}