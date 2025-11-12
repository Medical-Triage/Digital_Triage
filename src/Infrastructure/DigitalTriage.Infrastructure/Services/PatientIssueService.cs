using DigitalTriage.Application.Contracts.Services;
using DigitalTriage.Domain.Entities;
using DigitalTriage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriage.Infrastructure.Services;

/// <summary>
/// Provides CRUD operations for patient issues.
/// </summary>
internal sealed class PatientIssueService : IPatientIssueService
{
    private readonly MedicalTriageDbContext _dbContext;

    public PatientIssueService(MedicalTriageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PatientIssue>> GetByPatientIdAsync(int patientId)
    {
        var issues = await _dbContext.PatientIssues
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return issues;
    }

    public async Task<PatientIssue> CreateAsync(int patientId, string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var issue = new PatientIssue
        {
            PatientId = patientId,
            Title = title,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();
        return issue;
    }
}

