using DigitalTriage.Application.Contracts.Services;
using DigitalTriage.Domain.Entities;
using DigitalTriage.Infrastructure.Persistence;
using DigitalTriage.Infrastructure.Services;
using DigitalTriage.Tests.Helpers;
using DigitalTriage.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DigitalTriage.Tests.Services;

public class PatientIssueServiceTests : IDisposable
{
    private readonly MedicalTriageDbContext _dbContext;
    private readonly IPatientIssueService _patientIssueService;

    public PatientIssueServiceTests()
    {
        var fixture = new DatabaseFixture();
        _dbContext = fixture.CreateDbContext();
        _patientIssueService = new PatientIssueService(_dbContext);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesIssue()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.CreateAsync(
            patient.Id,
            "Test Issue",
            "This is a test issue description");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal("Test Issue", result.Title);
        Assert.True(result.IsActive);
        Assert.NotEqual(default(DateTimeOffset), result.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ThrowsException()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _patientIssueService.CreateAsync(patient.Id, "", "Description"));
    }

    [Fact]
    public async Task GetByPatientIdAsync_ReturnsAllIssuesForPatient()
    {
        // Arrange
        var patient1 = MockDataBuilder.CreatePatient();
        var patient2 = MockDataBuilder.CreatePatient();
        _dbContext.Patients.AddRange(patient1, patient2);
        await _dbContext.SaveChangesAsync();

        var issue1 = MockDataBuilder.CreatePatientIssue(patient1.Id, "Issue 1");
        var issue2 = MockDataBuilder.CreatePatientIssue(patient1.Id, "Issue 2");
        var issue3 = MockDataBuilder.CreatePatientIssue(patient2.Id, "Issue 3");
        _dbContext.PatientIssues.AddRange(issue1, issue2, issue3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.GetByPatientIdAsync(patient1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(patient1.Id, i.PatientId));
    }

    [Fact]
    public async Task GetByPatientIdAsync_ReturnsIssuesOrderedByCreatedDate()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue1 = MockDataBuilder.CreatePatientIssue(patient.Id, "Issue 1");
        issue1.CreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        var issue2 = MockDataBuilder.CreatePatientIssue(patient.Id, "Issue 2");
        issue2.CreatedAt = DateTimeOffset.UtcNow;
        var issue3 = MockDataBuilder.CreatePatientIssue(patient.Id, "Issue 3");
        issue3.CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        _dbContext.PatientIssues.AddRange(issue1, issue2, issue3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.GetByPatientIdAsync(patient.Id);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Issue 2", result[0].Title); // Most recent first
        Assert.Equal("Issue 3", result[1].Title);
        Assert.Equal("Issue 1", result[2].Title);
    }

    [Fact]
    public async Task UpdateAsync_WithProblemType_UpdatesIssue()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue = MockDataBuilder.CreatePatientIssue(patient.Id);
        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.UpdateAsync(
            issue.Id,
            problemType: "Cardiac");

        // Assert
        Assert.Equal("Cardiac", result.ProblemType);
        Assert.NotEqual(default(DateTimeOffset), result.LastUpdateDate);
    }

    [Fact]
    public async Task UpdateAsync_WithEmergencyGrade_UpdatesIssue()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue = MockDataBuilder.CreatePatientIssue(patient.Id);
        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.UpdateAsync(
            issue.Id,
            emergencyGrade: EsiLevel.Critical);

        // Assert
        Assert.Equal(EsiLevel.Critical, result.EmergencyGrade);
    }

    [Fact]
    public async Task UpdateAsync_WithIsActive_UpdatesIssue()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue = MockDataBuilder.CreatePatientIssue(patient.Id, isActive: true);
        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.UpdateAsync(
            issue.Id,
            isActive: false);

        // Assert
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithAllParameters_UpdatesAllFields()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue = MockDataBuilder.CreatePatientIssue(patient.Id);
        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        var originalUpdateDate = issue.LastUpdateDate;

        // Act
        var result = await _patientIssueService.UpdateAsync(
            issue.Id,
            problemType: "Respiratory",
            emergencyGrade: EsiLevel.Urgent,
            isActive: false);

        // Assert
        Assert.Equal("Respiratory", result.ProblemType);
        Assert.Equal(EsiLevel.Urgent, result.EmergencyGrade);
        Assert.False(result.IsActive);
        Assert.True(result.LastUpdateDate > originalUpdateDate);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentIssue_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _patientIssueService.UpdateAsync(99999, problemType: "Test"));
    }

    [Fact]
    public async Task CreateAsync_WithNullDescription_ThrowsException()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _patientIssueService.CreateAsync(patient.Id, "Test Title", null!));
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceTitle_ThrowsException()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _patientIssueService.CreateAsync(patient.Id, "   ", "Description"));
    }

    [Fact]
    public async Task GetByPatientIdAsync_WithNoIssues_ReturnsEmptyList()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patientIssueService.GetByPatientIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_WithOnlyProblemType_DoesNotChangeOtherFields()
    {
        // Arrange
        var patient = MockDataBuilder.CreatePatient();
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        var issue = MockDataBuilder.CreatePatientIssue(
            patient.Id,
            problemType: "Cardiac",
            emergencyGrade: EsiLevel.Urgent,
            isActive: true);
        _dbContext.PatientIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        var originalEmergencyGrade = issue.EmergencyGrade;
        var originalIsActive = issue.IsActive;

        // Act
        var result = await _patientIssueService.UpdateAsync(
            issue.Id,
            problemType: "Respiratory");

        // Assert
        Assert.Equal("Respiratory", result.ProblemType);
        Assert.Equal(originalEmergencyGrade, result.EmergencyGrade);
        Assert.Equal(originalIsActive, result.IsActive);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

