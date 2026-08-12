using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.ExternalServices;

public class CertificateService : ICertificateService
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(
        ResiliencePipelineProvider<string> pipelineProvider,
        HttpClient httpClient,
        ILogger<CertificateService> logger)
    {
        _pipelineProvider = pipelineProvider;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CertificateResult> IssueCertificateAsync(int studentId, string courseCode, CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline("certificate-api");

        return await pipeline.ExecuteAsync(async token =>
        {
            _logger.LogInformation("Requesting certificate for student {StudentId}, course {CourseCode}", studentId, courseCode);

            using var response = await _httpClient.PostAsJsonAsync(
                "/fake/certificates",
                new { StudentId = studentId, CourseCode = courseCode },
                token);

            if ((int)response.StatusCode >= 500)
                throw new HttpRequestException($"Upstream {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(token);
                throw new InvalidOperationException($"Certificate service rejected: {(int)response.StatusCode} {err}");
            }

            return await response.Content.ReadFromJsonAsync<CertificateResult>(cancellationToken: token)
                ?? throw new InvalidOperationException("Empty certificate response.");
        }, ct);
    }
}