using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalTriageApp.Models;

namespace DigitalTriageApp.Services
{
    /// <summary>
    /// Provides hospital management operations for both doctors and patients.
    /// </summary>
    public interface IHospitalService
    {
        Task<Hospital> CreateHospitalAsync(int doctorUserId, Hospital hospital);
        Task UpdateHospitalAsync(int doctorUserId, Hospital hospital);
        Task LeaveHospitalAsync(int doctorUserId, int hospitalId);
        Task JoinHospitalAsync(int doctorUserId, int hospitalId);
        Task<List<Hospital>> GetHospitalsForDoctorAsync(int doctorUserId);
        Task<List<Hospital>> SearchHospitalsAsync(string? searchTerm, string? country = null, string? county = null, string? city = null, int maxResults = 20);
        Task<List<Hospital>> GetAllAsync();
        Task<Hospital?> GetByIdAsync(int hospitalId);
        Task<Hospital?> GetHospitalWithDoctorsAsync(int hospitalId);
        Task<Hospital?> FindClosestHospitalAsync(Domicile? domicile);
        Task SetPatientPreferredHospitalAsync(int patientId, int hospitalId);
    }
}

