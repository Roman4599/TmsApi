using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Services;

public class TranscriptNotifier : ITranscriptNotifier
{
    private readonly IHubContext<TmsHub, ITmsHubClient> _hubContext;

    public TranscriptNotifier(IHubContext<TmsHub, ITmsHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTranscriptReadyAsync(int studentId, string reportId, string downloadUrl)
    {
        var groupName = $"student-{studentId}";
        await _hubContext.Clients
            .Group(groupName)
            .ReceiveTranscriptReady(reportId, downloadUrl);
    }
}