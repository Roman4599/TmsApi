namespace TmsApi.Application.Interfaces;

public interface ITranscriptNotifier
{
    Task NotifyTranscriptReadyAsync(int studentId, string reportId, string downloadUrl);
}