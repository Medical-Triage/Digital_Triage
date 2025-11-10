using BCrypt.Net;
using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Provides operations for patient registration, authentication, retrieval and updates.
 /// </summary>
 public class PatientService : IPatientService
 {
        private readonly MedicalTriageDbContext _db;
        private readonly ILogger<PatientService> _logger;
        private readonly IHospitalService _hospitalService;

        public PatientService(
            MedicalTriageDbContext db,
            ILogger<PatientService> logger,
            IHospitalService hospitalService)
        {
            _db = db;
            _logger = logger;
            _hospitalService = hospitalService;
        }

        public async Task<Patient?> GetByEmailAsync(string email)
        {
            return await _db.Patients
                .Include(p => p.PlaceOfBirth)
                .Include(p => p.Domicile)
                .Include(p => p.PreferredHospital)
                .Include(p => p.DoctorProfile)
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _db.Patients
                .Include(p => p.PlaceOfBirth)
                .Include(p => p.Domicile)
                .Include(p => p.PreferredHospital)
                .Include(p => p.DoctorProfile)
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
            Console.WriteLine($"Role: '{patient.Role}'");
            Console.WriteLine($"Password length: {password?.Length ?? 0}");
            Console.WriteLine($"PlaceOfBirth: {(patient.PlaceOfBirth != null ? "provided" : "null")}");
            Console.WriteLine($"Domicile: {(patient.Domicile != null ? "provided" : "null")}");

            patient.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

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

            var doctorProfile = patient.DoctorProfile;
            if (doctorProfile != null)
            {
                doctorProfile.Specialization = doctorProfile.Specialization.Trim();
                patient.DoctorProfile = null;
            }

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Patient saved with ID: {patient.Id}");

            if (doctorProfile != null)
            {
                doctorProfile.UserId = patient.Id;
                _db.DoctorProfiles.Add(doctorProfile);
                await _db.SaveChangesAsync();
                patient.DoctorProfile = doctorProfile;
            }

            var md = new MedicalData
            {
                PatientId = patient.Id,
                LastVisitDate = DateTime.UtcNow
            };
            _db.MedicalDatas.Add(md);
            await _db.SaveChangesAsync();

            if (patient.DomicileId.HasValue || patient.Domicile != null)
            {
                var domicile = patient.Domicile ?? await _db.Domiciles.FindAsync(patient.DomicileId);
                var closest = await _hospitalService.FindClosestHospitalAsync(domicile);
                if (closest != null)
                {
                    patient.PreferredHospitalId = closest.Id;
                    _db.Patients.Update(patient);
                    await _db.SaveChangesAsync();
                }
            }

            Console.WriteLine("=== PatientService: RegisterAsync completed successfully ===");
            return patient;
        }

        public async Task<Patient?> AuthenticateAsync(string email, string password)
        {
            Console.WriteLine("=== PatientService: AuthenticateAsync called ===");
            Console.WriteLine($"Email: '{email}', Password length: {password?.Length ?? 0}");

            var patient = await _db.Patients
                .Include(p => p.DoctorProfile)
                .Include(p => p.PreferredHospital)
                .FirstOrDefaultAsync(p => p.Email == email);

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
            patient.Role ??= patient.DoctorProfile != null ? "Doctor" : "Patient";
            Console.WriteLine($"=== PatientService: AuthenticateAsync completed - User: {patient.Email}, Role: {patient.Role} ===");
            return patient;
        }

 public async Task UpdateAsync(Patient patient)
 {
  Console.WriteLine("=== PatientService: UpdateAsync called ===");
  Console.WriteLine($"Patient ID: {patient.Id}, Email: '{patient.Email}'");
  
  // Handle PlaceOfBirth
  if (patient.PlaceOfBirth != null)
  {
   if (patient.PlaceOfBirth.Id == 0)
   {
    // New PlaceOfBirth - create it
    Console.WriteLine("Creating new PlaceOfBirth");
    _db.PlacesOfBirth.Add(patient.PlaceOfBirth);
    await _db.SaveChangesAsync();
    patient.PlaceOfBirthId = patient.PlaceOfBirth.Id;
    Console.WriteLine($"PlaceOfBirth created with ID: {patient.PlaceOfBirthId}");
   }
   else if (patient.PlaceOfBirthId.HasValue && patient.PlaceOfBirthId.Value == patient.PlaceOfBirth.Id)
   {
    // Existing PlaceOfBirth - update it
    Console.WriteLine($"Updating existing PlaceOfBirth with ID: {patient.PlaceOfBirth.Id}");
    _db.PlacesOfBirth.Update(patient.PlaceOfBirth);
   }
  }
  
  // Handle Domicile
  if (patient.Domicile != null)
  {
   if (patient.Domicile.Id == 0)
   {
    // New Domicile - create it
    Console.WriteLine("Creating new Domicile");
    _db.Domiciles.Add(patient.Domicile);
    await _db.SaveChangesAsync();
    patient.DomicileId = patient.Domicile.Id;
    Console.WriteLine($"Domicile created with ID: {patient.DomicileId}");
   }
   else if (patient.DomicileId.HasValue && patient.DomicileId.Value == patient.Domicile.Id)
   {
    // Existing Domicile - update it
    Console.WriteLine($"Updating existing Domicile with ID: {patient.Domicile.Id}");
    _db.Domiciles.Update(patient.Domicile);
   }
  }
  
  // Update the patient
  Console.WriteLine("Updating patient record");
  _db.Patients.Update(patient);

            if (patient.DoctorProfile != null)
            {
                if (patient.DoctorProfile.Id == 0)
                {
                    patient.DoctorProfile.UserId = patient.Id;
                    _db.DoctorProfiles.Add(patient.DoctorProfile);
                }
                else
                {
                    _db.DoctorProfiles.Update(patient.DoctorProfile);
                }
            }

  await _db.SaveChangesAsync();
  Console.WriteLine("=== PatientService: UpdateAsync completed successfully ===");
 }

        public async Task<List<Patient>> GetByHospitalIdsAsync(IEnumerable<int> hospitalIds)
        {
            var ids = hospitalIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids == null || ids.Count == 0)
            {
                return new List<Patient>();
            }

            return await _db.Patients
                .Include(p => p.MedicalDatas)
                .Include(p => p.Issues)
                .Include(p => p.PlaceOfBirth)
                .Include(p => p.Domicile)
                .Include(p => p.PreferredHospital)
                .Include(p => p.DoctorProfile)
                .Where(p => p.PreferredHospitalId.HasValue && ids.Contains(p.PreferredHospitalId.Value))
                .ToListAsync();
        }

        public Task<List<Patient>> GetAllAsync()
        {
            return _db.Patients
                .Include(p => p.MedicalDatas)
                .Include(p => p.Issues)
                .Include(p => p.PlaceOfBirth)
                .Include(p => p.Domicile)
                .Include(p => p.PreferredHospital)
                .Include(p => p.DoctorProfile)
                .ToListAsync();
        }

 public async Task DeleteAsync(int patientId)
 {
  Console.WriteLine($"=== PatientService: DeleteAsync called for patient ID: {patientId} ===");
  
  // Load patient with all related data
  var patient = await _db.Patients
   .Include(p => p.PlaceOfBirth)
   .Include(p => p.Domicile)
   .Include(p => p.MedicalDatas)
   .Include(p => p.Issues)
   .FirstOrDefaultAsync(p => p.Id == patientId);

  if (patient == null)
  {
   Console.WriteLine($"Patient with ID {patientId} not found");
   return;
  }

  Console.WriteLine($"Patient found - Email: '{patient.Email}'");

  // Handle PlaceOfBirth - delete if no other patients reference it
  if (patient.PlaceOfBirthId.HasValue)
  {
   var placeOfBirthId = patient.PlaceOfBirthId.Value;
   var otherPatientsUsingPlaceOfBirth = await _db.Patients
    .AnyAsync(p => p.Id != patientId && p.PlaceOfBirthId == placeOfBirthId);
   
   if (!otherPatientsUsingPlaceOfBirth && patient.PlaceOfBirth != null)
   {
    Console.WriteLine($"Deleting PlaceOfBirth with ID: {placeOfBirthId}");
    _db.PlacesOfBirth.Remove(patient.PlaceOfBirth);
   }
   else
   {
    Console.WriteLine($"PlaceOfBirth with ID {placeOfBirthId} is used by other patients, keeping it");
   }
  }

  // Handle Domicile - delete if no other patients reference it
  if (patient.DomicileId.HasValue)
  {
   var domicileId = patient.DomicileId.Value;
   var otherPatientsUsingDomicile = await _db.Patients
    .AnyAsync(p => p.Id != patientId && p.DomicileId == domicileId);
   
   if (!otherPatientsUsingDomicile && patient.Domicile != null)
   {
    Console.WriteLine($"Deleting Domicile with ID: {domicileId}");
    _db.Domiciles.Remove(patient.Domicile);
   }
   else
   {
    Console.WriteLine($"Domicile with ID {domicileId} is used by other patients, keeping it");
   }
  }

  // Delete patient (MedicalData and PatientIssue will be cascade deleted)
  Console.WriteLine("Deleting patient and related data (MedicalData and PatientIssue will be cascade deleted)");
  _db.Patients.Remove(patient);
  await _db.SaveChangesAsync();
  
  Console.WriteLine($"=== PatientService: DeleteAsync completed successfully for patient ID: {patientId} ===");
 }
 }
}
