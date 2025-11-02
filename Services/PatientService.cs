using BCrypt.Net;
using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Provides operations for patient registration, authentication, retrieval and updates.
 /// </summary>
 public class PatientService : IPatientService
 {
 private readonly MedicalTriageDbContext _db;
 private readonly ILogger<PatientService> _logger;

 public PatientService(MedicalTriageDbContext db, ILogger<PatientService> logger)
 {
 _db = db;
 _logger = logger;
 }

 public async Task<Patient?> GetByEmailAsync(string email)
 {
 return await _db.Patients
 .Include(p => p.PlaceOfBirth)
 .Include(p => p.Domicile)
 .FirstOrDefaultAsync(p => p.Email == email);
 }

 public async Task<Patient?> GetByIdAsync(int id)
 {
 return await _db.Patients
 .Include(p => p.PlaceOfBirth)
 .Include(p => p.Domicile)
 .FirstOrDefaultAsync(p => p.Id == id);
 }

 public Task<bool> IsEmailTakenAsync(string email)
 {
 return _db.Patients.AnyAsync(p => p.Email == email);
 }

 public async Task<Patient> RegisterAsync(Patient patient, string password)
 {
 // Hash password
 patient.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

 // Create related records if provided
 if (patient.PlaceOfBirth != null)
 {
 _db.PlacesOfBirth.Add(patient.PlaceOfBirth);
 await _db.SaveChangesAsync();
 patient.PlaceOfBirthId = patient.PlaceOfBirth.Id;
 }
 if (patient.Domicile != null)
 {
 _db.Domiciles.Add(patient.Domicile);
 await _db.SaveChangesAsync();
 patient.DomicileId = patient.Domicile.Id;
 }

 _db.Patients.Add(patient);
 await _db.SaveChangesAsync();

 // Create empty MedicalData
 var md = new MedicalData
 {
 PatientId = patient.Id,
 LastVisitDate = DateTime.UtcNow
 };
 _db.MedicalDatas.Add(md);
 await _db.SaveChangesAsync();

 return patient;
 }

 public async Task<Patient?> AuthenticateAsync(string email, string password)
 {
 // Doctor hard-coded check (demo only)
 if (email.EndsWith("@hospital.com", StringComparison.OrdinalIgnoreCase) && password == "hospital")
 {
 return new Patient
 {
 Id = -1, // pseudo id for doctor (no DB row)
 Email = email,
 FirstName = "Doctor",
 LastName = "User",
 Role = "Doctor",
 PasswordHash = string.Empty
 };
 }

 var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email);
 if (patient == null) return null;

 var ok = BCrypt.Net.BCrypt.Verify(password, patient.PasswordHash);
 if (!ok) return null;

 patient.Role ??= "Patient";
 return patient;
 }

 public async Task UpdateAsync(Patient patient)
 {
 // Attach and update related entities
 if (patient.PlaceOfBirthId.HasValue && patient.PlaceOfBirth != null)
 {
 _db.PlacesOfBirth.Update(patient.PlaceOfBirth);
 }
 if (patient.DomicileId.HasValue && patient.Domicile != null)
 {
 _db.Domiciles.Update(patient.Domicile);
 }
 _db.Patients.Update(patient);
 await _db.SaveChangesAsync();
 }

 public Task<List<Patient>> GetAllAsync()
 {
 return _db.Patients
 .Include(p => p.MedicalDatas)
 .Include(p => p.Issues)
 .Include(p => p.PlaceOfBirth)
 .Include(p => p.Domicile)
 .ToListAsync();
 }
 }
}
