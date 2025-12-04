using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using Modelsproject;   // ✅ Import your models
using ReportDemo.Models;

namespace ReportDemo.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ Your Students table
        public DbSet<Student> Students { get; set; } = null!;
        
        // ✅ Classes table
        public DbSet<Class> Classes { get; set; } = null!;
        
        // ✅ Exam Results table
        public DbSet<ExamResult> ExamResults { get; set; } = null!;
        
        // ✅ Promotion History table
        public DbSet<PromotionHistory> PromotionHistories { get; set; } = null!;
        
        // ✅ Alumni table
        public DbSet<Alumni> Alumni { get; set; } = null!;
        
        // ✅ User Profiles table
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        
        // ✅ Sessions table
        public DbSet<Session> Sessions { get; set; } = null!;
        
        // ✅ Sections table
        public DbSet<Section> Sections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Class entity
            modelBuilder.Entity<Class>(entity =>
            {
                // Configure timestamps to auto-update
                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAddOrUpdate();
            });

            // Configure Session entity
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique();
                entity.HasIndex(s => s.AcademicYear);
                entity.HasIndex(s => s.IsCurrent);
            });

            // Configure Section entity
            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasIndex(s => new { s.ClassId, s.Name }).IsUnique();
                entity.HasOne(s => s.Class)
                    .WithMany()
                    .HasForeignKey(s => s.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Student entity
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Section)
                    .WithMany()
                    .HasForeignKey(s => s.SectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Class)
                    .WithMany()
                    .HasForeignKey(s => s.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PromotionHistory entity
            modelBuilder.Entity<PromotionHistory>(entity =>
            {
                entity.HasOne(ph => ph.Student)
                    .WithMany()
                    .HasForeignKey(ph => ph.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ph => ph.OldClass)
                    .WithMany()
                    .HasForeignKey(ph => ph.OldClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ph => ph.NewClass)
                    .WithMany()
                    .HasForeignKey(ph => ph.NewClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Class entity
            modelBuilder.Entity<Class>(entity =>
            {
                // Configure timestamps to auto-update
                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            // Configure Student entity
            modelBuilder.Entity<Student>(entity =>
            {
                // Ensure RollNumber is unique
                entity.HasIndex(s => s.RollNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Students_RollNumber");

                // Ensure Email is unique
                entity.HasIndex(s => s.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Students_Email");

                // Configure foreign key relationship with Class
                entity.HasOne(s => s.Class)
                    .WithMany(c => c.Students)
                    .HasForeignKey(s => s.ClassId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

                // Configure timestamps to auto-update
                entity.Property(s => s.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(s => s.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            // Configure UserProfile relationship
            modelBuilder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserProfile>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique UserId in UserProfiles
            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            // Configure ExamResult entity
            modelBuilder.Entity<ExamResult>(entity =>
            {
                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Class)
                    .WithMany()
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Ensure unique exam per student per term per academic year
                entity.HasIndex(e => new { e.StudentId, e.Term, e.AcademicYear })
                    .IsUnique()
                    .HasDatabaseName("IX_ExamResults_Student_Term_Year");
            });

            // Configure PromotionHistory entity
            modelBuilder.Entity<PromotionHistory>(entity =>
            {
                entity.HasOne(p => p.Student)
                    .WithMany()
                    .HasForeignKey(p => p.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.OldClass)
                    .WithMany()
                    .HasForeignKey(p => p.OldClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.NewClass)
                    .WithMany()
                    .HasForeignKey(p => p.NewClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            // Configure Alumni entity
            modelBuilder.Entity<Alumni>(entity =>
            {
                entity.HasOne(a => a.GraduatedFromClass)
                    .WithMany()
                    .HasForeignKey(a => a.GraduatedFromClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ensure unique original student ID
                entity.HasIndex(a => a.OriginalStudentId)
                    .IsUnique()
                    .HasDatabaseName("IX_Alumni_OriginalStudentId");

                // Ensure unique email for alumni
                entity.HasIndex(a => a.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Alumni_Email");

                entity.Property(a => a.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(a => a.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });
        }
    }
}
