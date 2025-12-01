using Microsoft.EntityFrameworkCore;
using UserManagementSystem.Core.Entities;

namespace UserManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasOne(d => d.User)
                      .WithMany(u => u.Documents)
                      .HasForeignKey(d => d.UserId);

                entity.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(d => d.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<BackgroundJob>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.JobId).IsUnique();
            });
        }
    }
}
