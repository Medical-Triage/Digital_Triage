using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Provides CRUD operations for patient issues.
 /// </summary>
 public class PatientIssueService : IPatientIssueService
 {
 private readonly MedicalTriageDbContext _db;
 public PatientIssueService(MedicalTriageDbContext db)
 {
 _db = db;
 }

 public Task<List<PatientIssue>> GetByPatientIdAsync(int patientId)
 {
 return _db.PatientIssues.Where(i => i.PatientId == patientId)
 .OrderByDescending(i => i.CreatedAt)
 .ToListAsync();
 }

 public async Task<PatientIssue> CreateAsync(int patientId, string title, string description)
 {
 var issue = new PatientIssue
 {
 PatientId = patientId,
 Title = title,
 Description = description,
 CreatedAt = DateTimeOffset.UtcNow
 };
 _db.PatientIssues.Add(issue);
 await _db.SaveChangesAsync();
 return issue;
 }
 }
}
