using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Provides medical data retrieval and persistence.
 /// </summary>
 public class MedicalDataService : IMedicalDataService
 {
 private readonly MedicalTriageDbContext _db;
 public MedicalDataService(MedicalTriageDbContext db)
 {
 _db = db;
 }

 public Task<MedicalData?> GetByPatientIdAsync(int patientId)
 {
 return _db.MedicalDatas.FirstOrDefaultAsync(m => m.PatientId == patientId);
 }

 public async Task UpdateAsync(MedicalData data)
 {
 _db.MedicalDatas.Update(data);
 await _db.SaveChangesAsync();
 }
 }
}
