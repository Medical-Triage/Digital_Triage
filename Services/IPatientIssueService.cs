using DigitalTriageApp.Models;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Defines operations to manage patient issues/problems.
 /// </summary>
 public interface IPatientIssueService
 {
 Task<List<PatientIssue>> GetByPatientIdAsync(int patientId);
 Task<PatientIssue> CreateAsync(int patientId, string title, string description);
 }
}
