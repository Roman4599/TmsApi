using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Infrastructure.Workers;

public class TranscriptWorker : Microsoft.Extensions.Hosting.BackgroundService   // ← Full namespace to avoid ambiguity
{
    private readonly Channel<TranscriptRequest> _channel;
    private readonly ITranscriptStatusStore _statusStore;
    private readonly ITranscriptNotifier _notifier;
    private readonly ILogger<TranscriptWorker> _logger;

    public TranscriptWorker(
        Channel<TranscriptRequest> channel,
        ITranscriptStatusStore statusStore,
        ITranscriptNotifier notifier,
        ILogger<TranscriptWorker> logger)
    {
        _channel = channel;
        _statusStore = statusStore;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Transcript worker started.");

        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            var reportId = request.ReportId!;
            try
            {
                await _statusStore.MarkProcessingAsync(reportId, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                var downloadUrl = $"/api/v2/transcripts/{reportId}/download";
                await _statusStore.MarkReadyAsync(reportId, downloadUrl, stoppingToken);
                await _notifier.NotifyTranscriptReadyAsync(request.StudentId, reportId, downloadUrl);
                _logger.LogInformation("Transcript ready: {ReportId}", reportId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("Worker shutdown – transcript {ReportId} did not complete", reportId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate transcript {ReportId}", reportId);
                await _statusStore.MarkFailedAsync(reportId, ex.Message, CancellationToken.None);
            }
        }
    }
}