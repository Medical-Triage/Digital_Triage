using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigitalTriageApp.Services
{
    /// <summary>
    /// Implements hospital management operations for doctors and patients.
    /// </summary>
    public class HospitalService : IHospitalService
    {
        private readonly MedicalTriageDbContext _db;
        private readonly ILogger<HospitalService> _logger;

        public HospitalService(MedicalTriageDbContext db, ILogger<HospitalService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Hospital> CreateHospitalAsync(int doctorUserId, Hospital hospital)
        {
            var doctorProfile = await EnsureDoctorProfileAsync(doctorUserId);

            hospital.Name = hospital.Name.Trim();
            hospital.CreatedByDoctorId = doctorProfile.Id;

            _db.Hospitals.Add(hospital);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Hospital {HospitalName} (ID: {HospitalId}) created by doctor user {DoctorUserId}", hospital.Name, hospital.Id, doctorUserId);
            return hospital;
        }

        public async Task UpdateHospitalAsync(int doctorUserId, Hospital hospital)
        {
            var doctorProfile = await EnsureDoctorProfileAsync(doctorUserId);
            var existing = await _db.Hospitals.FirstOrDefaultAsync(h => h.Id == hospital.Id);

            if (existing == null)
            {
                throw new InvalidOperationException("Hospital not found.");
            }

            if (!await IsDoctorActiveInHospitalAsync(doctorProfile.Id, hospital.Id))
            {
                throw new UnauthorizedAccessException("Doctor is not an active member of this hospital.");
            }

            existing.Name = hospital.Name.Trim();
            existing.Country = hospital.Country;
            existing.County = hospital.County;
            existing.City = hospital.City;
            existing.Street = hospital.Street;
            existing.Number = hospital.Number;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Hospital {HospitalId} updated by doctor user {DoctorUserId}", hospital.Id, doctorUserId);
        }

        public async Task LeaveHospitalAsync(int doctorUserId, int hospitalId)
        {
            var doctorProfile = await EnsureDoctorProfileAsync(doctorUserId);
            var membership = await _db.DoctorHospitalMemberships
                .FirstOrDefaultAsync(m => m.DoctorId == doctorProfile.Id && m.HospitalId == hospitalId && m.IsActive);

            if (membership == null)
            {
                throw new InvalidOperationException("Doctor is not an active member of the specified hospital.");
            }

            membership.IsActive = false;
            membership.LeftAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Doctor user {DoctorUserId} left hospital {HospitalId}", doctorUserId, hospitalId);

            var hasOtherMembers = await _db.DoctorHospitalMemberships
                .AnyAsync(m => m.HospitalId == hospitalId && m.IsActive);

            if (!hasOtherMembers)
            {
                var hospital = await _db.Hospitals.FirstOrDefaultAsync(h => h.Id == hospitalId);
                if (hospital != null)
                {
                    _db.Hospitals.Remove(hospital);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Hospital {HospitalId} deleted because it no longer had active doctors", hospitalId);
                }
            }
        }

        public async Task JoinHospitalAsync(int doctorUserId, int hospitalId)
        {
            var doctorProfile = await EnsureDoctorProfileAsync(doctorUserId);
            var hospital = await _db.Hospitals.FirstOrDefaultAsync(h => h.Id == hospitalId)
                ?? throw new InvalidOperationException("Hospital not found.");

            var membership = await _db.DoctorHospitalMemberships
                .FirstOrDefaultAsync(m => m.DoctorId == doctorProfile.Id && m.HospitalId == hospitalId);

            if (membership == null)
            {
                membership = new DoctorHospitalMembership
                {
                    DoctorId = doctorProfile.Id,
                    HospitalId = hospitalId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _db.DoctorHospitalMemberships.Add(membership);
            }
            else
            {
                membership.IsActive = true;
                membership.JoinedAt = DateTime.UtcNow;
                membership.LeftAt = null;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Doctor user {DoctorUserId} joined hospital {HospitalId}", doctorUserId, hospitalId);
        }

        public async Task<List<Hospital>> GetHospitalsForDoctorAsync(int doctorUserId)
        {
            var doctorProfile = await EnsureDoctorProfileAsync(doctorUserId);
            return await _db.DoctorHospitalMemberships
                .Where(m => m.DoctorId == doctorProfile.Id && m.IsActive)
                .Select(m => m.Hospital)
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public async Task<List<Hospital>> SearchHospitalsAsync(string? searchTerm, string? country = null, string? county = null, string? city = null, int maxResults = 20)
        {
            var query = _db.Hospitals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = $"%{searchTerm.Trim()}%";
                query = query.Where(h =>
                    EF.Functions.Like(h.Name, term) ||
                    (h.City != null && EF.Functions.Like(h.City, term)) ||
                    (h.County != null && EF.Functions.Like(h.County, term)) ||
                    (h.Country != null && EF.Functions.Like(h.Country, term)));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                var countryTerm = $"%{country.Trim()}%";
                query = query.Where(h => h.Country != null && EF.Functions.Like(h.Country, countryTerm));
            }

            if (!string.IsNullOrWhiteSpace(county))
            {
                var countyTerm = $"%{county.Trim()}%";
                query = query.Where(h => h.County != null && EF.Functions.Like(h.County, countyTerm));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = $"%{city.Trim()}%";
                query = query.Where(h => h.City != null && EF.Functions.Like(h.City, cityTerm));
            }

            return await query
                .OrderBy(h => h.Name)
                .Take(maxResults)
                .ToListAsync();
        }

        public Task<List<Hospital>> GetAllAsync()
        {
            return _db.Hospitals
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public Task<Hospital?> GetByIdAsync(int hospitalId)
        {
            return _db.Hospitals.FirstOrDefaultAsync(h => h.Id == hospitalId);
        }

        public Task<Hospital?> GetHospitalWithDoctorsAsync(int hospitalId)
        {
            return _db.Hospitals
                .Include(h => h.DoctorMemberships.Where(m => m.IsActive))
                    .ThenInclude(m => m.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(h => h.Id == hospitalId);
        }

        public async Task<Hospital?> FindClosestHospitalAsync(Domicile? domicile)
        {
            var hospitals = await _db.Hospitals.ToListAsync();
            if (hospitals.Count == 0)
            {
                return null;
            }

            if (domicile == null)
            {
                return hospitals.First();
            }

            string? normCountry = Normalize(domicile.Country);
            string? normCounty = Normalize(domicile.County);
            string? normCity = Normalize(domicile.City);
            string? normStreet = Normalize(domicile.Street);
            string? normNumber = Normalize(domicile.Number);

            var ranked = hospitals
                .Select(h => new
                {
                    Hospital = h,
                    Score = CalculateMatchScore(h, normCountry, normCounty, normCity, normStreet, normNumber)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Hospital.Id)
                .FirstOrDefault();

            if (ranked == null)
            {
                return hospitals.First();
            }

            return ranked.Score > 0 ? ranked.Hospital : hospitals.First();
        }

        public async Task SetPatientPreferredHospitalAsync(int patientId, int hospitalId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId)
                ?? throw new InvalidOperationException("Patient not found.");

            var hospital = await _db.Hospitals.FirstOrDefaultAsync(h => h.Id == hospitalId)
                ?? throw new InvalidOperationException("Hospital not found.");

            patient.PreferredHospitalId = hospitalId;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Patient {PatientId} assigned to hospital {HospitalId}", patientId, hospitalId);
        }

        private async Task<DoctorProfile> EnsureDoctorProfileAsync(int doctorUserId)
        {
            var profile = await _db.DoctorProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == doctorUserId);

            if (profile == null)
            {
                throw new InvalidOperationException("Doctor profile not found for the current user.");
            }

            return profile;
        }

        private async Task<bool> IsDoctorActiveInHospitalAsync(int doctorProfileId, int hospitalId)
        {
            return await _db.DoctorHospitalMemberships.AnyAsync(m =>
                m.DoctorId == doctorProfileId &&
                m.HospitalId == hospitalId &&
                m.IsActive);
        }

        private static int CalculateMatchScore(Hospital hospital, string? country, string? county, string? city, string? street, string? number)
        {
            int score = 0;

            if (!string.IsNullOrEmpty(country) && country == Normalize(hospital.Country))
            {
                score += 1;
            }

            if (!string.IsNullOrEmpty(county) && county == Normalize(hospital.County))
            {
                score += 2;
            }

            if (!string.IsNullOrEmpty(city) && city == Normalize(hospital.City))
            {
                score += 3;
            }

            if (!string.IsNullOrEmpty(street) && street == Normalize(hospital.Street))
            {
                score += 4;
            }

            if (!string.IsNullOrEmpty(number) && number == Normalize(hospital.Number))
            {
                score += 5;
            }

            return score;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }
    }
}

