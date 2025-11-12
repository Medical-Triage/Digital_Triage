using System.ComponentModel.DataAnnotations;

namespace DigitalTriage.Domain.Entities;

/// <summary>
/// Represents a problem/issue reported by a patient.
/// </summary>
public class PatientIssue
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
