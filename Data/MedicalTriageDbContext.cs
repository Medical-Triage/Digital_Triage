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

 modelBuilder.Entity<MedicalData>()
 .HasOne(md => md.Patient)
 .WithMany(p => p.MedicalDatas)
 .HasForeignKey(md => md.PatientId)
 .OnDelete(DeleteBehavior.Cascade);

 modelBuilder.Entity<PatientIssue>()
 .HasOne(i => i.Patient)
 .WithMany(p => p.Issues)
 .HasForeignKey(i => i.PatientId)
 .OnDelete(DeleteBehavior.Cascade);
 }
 }
}
