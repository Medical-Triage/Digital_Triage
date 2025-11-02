using DigitalTriageApp.Models;

namespace DigitalTriageApp.Services
{
 /// <summary>
 /// Defines operations to manage patient medical data.
 /// </summary>
 public interface IMedicalDataService
 {
 Task<MedicalData?> GetByPatientIdAsync(int patientId);
 Task UpdateAsync(MedicalData data);
 }
}
