using DigitalTriageApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriageApp.Data
{
 /// <summary>
 /// Provides the EF Core database context for the MedicalTriage application, including Patients, Places, MedicalData, and PatientIssues.
 /// </summary>
 /// <remarks>
 /// This context uses SQL Server as configured in Program.cs via the 'MedicalTriageDb' connection string.
 /// </remarks>
 public class MedicalTriageDbContext : DbContext
 {
 public MedicalTriageDbContext(DbContextOptions<MedicalTriageDbContext> options) : base(options)
 {
 }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<MedicalData> MedicalDatas => Set<MedicalData>();
        public DbSet<PlaceOfBirth> PlacesOfBirth => Set<PlaceOfBirth>();
        public DbSet<Domicile> Domiciles => Set<Domicile>();
        public DbSet<PatientIssue> PatientIssues => Set<PatientIssue>();
        public DbSet<Hospital> Hospitals => Set<Hospital>();
        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
        public DbSet<DoctorHospitalMembership> DoctorHospitalMemberships => Set<DoctorHospitalMembership>();
        public DbSet<MedicalFile> MedicalFiles => Set<MedicalFile>();

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
 base.OnModelCreating(modelBuilder);

 // Relations
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PlaceOfBirth)
                .WithMany()
                .HasForeignKey(p => p.PlaceOfBirthId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Domicile)
                .WithMany()
                .HasForeignKey(p => p.DomicileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PreferredHospital)
                .WithMany(h => h.AssignedPatients)
                .HasForeignKey(p => p.PreferredHospitalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MedicalData>()
                .HasOne(md => md.Patient)
                .WithMany(p => p.MedicalDatas)
                .HasForeignKey(md => md.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalData>()
                .HasOne(md => md.AuthorizedDoctor)
                .WithMany(d => d.AuthorizedMedicalData)
                .HasForeignKey(md => md.AuthorizedDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MedicalData>()
                .Property(md => md.IsConfidential)
                .HasDefaultValue(true);

            modelBuilder.Entity<MedicalData>()
                .Property(md => md.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<MedicalFile>()
                .HasOne(f => f.MedicalData)
                .WithMany(md => md.Files)
                .HasForeignKey(f => f.MedicalDataId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalFile>()
                .Property(f => f.UploadDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<PatientIssue>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Issues)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorProfile>()
                .HasOne(dp => dp.User)
                .WithOne(p => p.DoctorProfile)
                .HasForeignKey<DoctorProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hospital>()
                .HasOne(h => h.CreatedByDoctor)
                .WithMany(d => d.CreatedHospitals)
                .HasForeignKey(h => h.CreatedByDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DoctorHospitalMembership>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.HospitalMemberships)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorHospitalMembership>()
                .HasOne(m => m.Hospital)
                .WithMany(h => h.DoctorMemberships)
                .HasForeignKey(m => m.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorHospitalMembership>()
                .HasIndex(m => new { m.DoctorId, m.HospitalId, m.IsActive })
                .HasDatabaseName("IX_DoctorHospitalMembership_Doctor_Hospital_IsActive");
 }
 }
}
