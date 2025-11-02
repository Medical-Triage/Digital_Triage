using System.ComponentModel.DataAnnotations;

namespace DigitalTriageApp.Models
{
 /// <summary>
 /// Represents medical data associated with a patient.
 /// </summary>
 public class MedicalData
 {
 public int Id { get; set; }

 [MaxLength(5)]
 public string? BloodType { get; set; }

 [MaxLength(1000)]
 public string? Allergies { get; set; }

 [MaxLength(1000)]
 public string? ChronicDiseases { get; set; }

 [MaxLength(1000)]
 public string? CurrentMedication { get; set; }

 [MaxLength(200)]
 public string? EmergencyContactName { get; set; }

 [MaxLength(20)]
 public string? EmergencyContactPhone { get; set; }

 public DateTime? LastVisitDate { get; set; }

 [MaxLength(100)]
 public string? TriageCategory { get; set; }

 public int PatientId { get; set; }
 public Patient? Patient { get; set; }
 }
}
