using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record EnrollStudentRequest
{
    [Range(1, int.MaxValue)]
    public required int StudentId { get; init; }
}