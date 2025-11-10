using System;
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
 return _db.MedicalDatas
 .Include(m => m.Files)
 .Include(m => m.AuthorizedDoctor)
 .ThenInclude(d => d.User)
 .FirstOrDefaultAsync(m => m.PatientId == patientId);
 }

 public async Task UpdateAsync(MedicalData data)
 {
 data.UpdatedAt = DateTime.UtcNow;
 _db.MedicalDatas.Update(data);
 await _db.SaveChangesAsync();
 }

 public async Task<MedicalFile> AddFileAsync(int medicalDataId, string fileName, string filePath)
 {
 var file = new MedicalFile
 {
 MedicalDataId = medicalDataId,
 FileName = fileName,
 FilePath = filePath,
 UploadDate = DateTime.UtcNow
 };

 _db.MedicalFiles.Add(file);
 await _db.SaveChangesAsync();
 return file;
 }

 public async Task RemoveFileAsync(int fileId)
 {
 var file = await _db.MedicalFiles.FindAsync(fileId);
 if (file == null)
 {
 return;
 }

 _db.MedicalFiles.Remove(file);
 await _db.SaveChangesAsync();
 }

 public Task<MedicalFile?> GetFileAsync(int fileId)
 {
 return _db.MedicalFiles
 .Include(f => f.MedicalData)
 .FirstOrDefaultAsync(f => f.Id == fileId);
 }
 }
}
