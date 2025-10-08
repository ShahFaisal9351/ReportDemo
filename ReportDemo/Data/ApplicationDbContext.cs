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
        
        // ✅ User Profiles table
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}
