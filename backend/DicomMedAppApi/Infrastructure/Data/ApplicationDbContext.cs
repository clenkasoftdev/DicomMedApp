using Clenkasoft.DicomMedAppApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Clenkasoft.DicomMedAppApi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Study> Studies => Set<Study>();
        public DbSet<Series> Series => Set<Series>();
        public DbSet<Instance> Instances => Set<Instance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient Configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PatientId);
                entity.Property(e => e.PatientId).HasMaxLength(64).IsRequired();
                entity.Property(e => e.PatientName).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Gender).HasMaxLength(16);
            });

            // Study Configuration
            modelBuilder.Entity<Study>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.StudyInstanceUid).IsUnique();
                entity.Property(e => e.StudyInstanceUid).HasMaxLength(128).IsRequired();
                entity.Property(e => e.StudyDescription).HasMaxLength(512);
                entity.Property(e => e.AccessionNumber).HasMaxLength(64);
                entity.Property(e => e.ReferringPhysicianName).HasMaxLength(256);

                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Studies)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Series Configuration
            modelBuilder.Entity<Series>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SeriesInstanceUid).IsUnique();
                entity.Property(e => e.SeriesInstanceUid).HasMaxLength(128).IsRequired();
                entity.Property(e => e.Modality).HasMaxLength(16);
                entity.Property(e => e.SeriesDescription).HasMaxLength(512);
                entity.Property(e => e.BodyPartExamined).HasMaxLength(64);
                entity.Property(e => e.ProtocolName).HasMaxLength(256);

                entity.HasOne(e => e.Study)
                    .WithMany(s => s.Series)
                    .HasForeignKey(e => e.StudyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Instance Configuration
            modelBuilder.Entity<Instance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SopInstanceUid).IsUnique();
                entity.Property(e => e.SopInstanceUid).HasMaxLength(128).IsRequired();
                entity.Property(e => e.SopClassUid).HasMaxLength(128);
                entity.Property(e => e.FilePath).HasMaxLength(1024).IsRequired();
                entity.Property(e => e.TransferSyntaxUid).HasMaxLength(128);

                entity.HasOne(e => e.Series)
                    .WithMany(s => s.Instances)
                    .HasForeignKey(e => e.SeriesId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Patient || e.Entity is Study || e.Entity is Series || e.Entity is Instance &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Patient patient)
                        patient.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Study study)
                        study.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Series series)
                        series.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Instance instance)
                        instance.CreatedAt = DateTime.UtcNow;
                }

                if (entry.Entity is Patient p)
                    p.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Study st)
                    st.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Series se)
                    se.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Instance i)
                    i.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
