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

 public async Task<bool> IsEmailTakenAsync(string email)
 {
  Console.WriteLine($"=== PatientService: IsEmailTakenAsync called with email: '{email}' ===");
  var result = await _db.Patients.AnyAsync(p => p.Email == email);
  Console.WriteLine($"Email '{email}' taken: {result}");
  return result;
 }

 public async Task<Patient> RegisterAsync(Patient patient, string password)
 {
  Console.WriteLine("=== PatientService: RegisterAsync called ===");
  Console.WriteLine($"Patient data - Email: '{patient.Email}', FirstName: '{patient.FirstName}', LastName: '{patient.LastName}'");
  Console.WriteLine($"Password length: {password?.Length ?? 0}");
  Console.WriteLine($"PlaceOfBirth: {(patient.PlaceOfBirth != null ? "provided" : "null")}");
  Console.WriteLine($"Domicile: {(patient.Domicile != null ? "provided" : "null")}");
  
  // Hash password
  Console.WriteLine("Hashing password...");
  patient.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
  Console.WriteLine("Password hashed successfully");

  // Create related records if provided
  if (patient.PlaceOfBirth != null)
  {
   Console.WriteLine($"Adding PlaceOfBirth - Country: '{patient.PlaceOfBirth.Country}', County: '{patient.PlaceOfBirth.County}', City: '{patient.PlaceOfBirth.City}'");
   _db.PlacesOfBirth.Add(patient.PlaceOfBirth);
   await _db.SaveChangesAsync();
   patient.PlaceOfBirthId = patient.PlaceOfBirth.Id;
   Console.WriteLine($"PlaceOfBirth saved with ID: {patient.PlaceOfBirthId}");
  }
  if (patient.Domicile != null)
  {
   Console.WriteLine($"Adding Domicile - Country: '{patient.Domicile.Country}', County: '{patient.Domicile.County}', City: '{patient.Domicile.City}'");
   _db.Domiciles.Add(patient.Domicile);
   await _db.SaveChangesAsync();
   patient.DomicileId = patient.Domicile.Id;
   Console.WriteLine($"Domicile saved with ID: {patient.DomicileId}");
  }

  Console.WriteLine("Adding patient to database...");
  _db.Patients.Add(patient);
  await _db.SaveChangesAsync();
  Console.WriteLine($"Patient saved with ID: {patient.Id}");

  // Create empty MedicalData
  Console.WriteLine("Creating MedicalData...");
  var md = new MedicalData
  {
   PatientId = patient.Id,
   LastVisitDate = DateTime.UtcNow
  };
  _db.MedicalDatas.Add(md);
  await _db.SaveChangesAsync();
  Console.WriteLine("MedicalData created successfully");

  Console.WriteLine("=== PatientService: RegisterAsync completed successfully ===");
  return patient;
 }

 public async Task<Patient?> AuthenticateAsync(string email, string password)
 {
  Console.WriteLine("=== PatientService: AuthenticateAsync called ===");
  Console.WriteLine($"Email: '{email}', Password length: {password?.Length ?? 0}");
  
  // Doctor hard-coded check (demo only)
  if (email.EndsWith("@hospital.com", StringComparison.OrdinalIgnoreCase) && password == "hospital")
  {
   Console.WriteLine("Doctor authentication detected (hard-coded check)");
   var doctor = new Patient
   {
    Id = -1, // pseudo id for doctor (no DB row)
    Email = email,
    FirstName = "Doctor",
    LastName = "User",
    Role = "Doctor",
    PasswordHash = string.Empty
   };
   Console.WriteLine("Doctor authentication successful");
   return doctor;
  }

  Console.WriteLine("Looking up patient in database...");
  var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email);
  if (patient == null)
  {
   Console.WriteLine($"Patient not found with email: '{email}'");
   return null;
  }
  
  Console.WriteLine($"Patient found - ID: {patient.Id}, Email: {patient.Email}, Role: {patient.Role}");
  Console.WriteLine("Verifying password...");
  var ok = BCrypt.Net.BCrypt.Verify(password, patient.PasswordHash);
  if (!ok)
  {
   Console.WriteLine("Password verification failed");
   return null;
  }

  Console.WriteLine("Password verification successful");
  patient.Role ??= "Patient";
  Console.WriteLine($"=== PatientService: AuthenticateAsync completed - User: {patient.Email}, Role: {patient.Role} ===");
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
