using KunFarm.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace KunFarm.DAL.Data
{
    public class KunFarmDbContext : DbContext
    {
        public KunFarmDbContext(DbContextOptions<KunFarmDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PlayerState> PlayerStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
                
                // One-to-one relationship with PlayerState
                entity.HasOne(u => u.PlayerState)
                      .WithOne(ps => ps.User)
                      .HasForeignKey<PlayerState>(ps => ps.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PlayerState entity configuration
            modelBuilder.Entity<PlayerState>(entity =>
            {
                entity.HasKey(e => e.UserId);
                
                entity.Property(e => e.LastSaved)
                      .HasDefaultValue(DateTime.Now);
                      
                entity.Property(e => e.Money)
                      .HasDefaultValue(0);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed data sẽ được thực hiện thông qua DatabaseSeeder service
            // để có thể sử dụng cấu hình từ appsettings.json
        }
    }
} 