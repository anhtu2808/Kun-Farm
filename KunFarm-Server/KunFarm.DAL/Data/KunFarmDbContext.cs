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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
                
                entity.Property(e => e.Coins).HasPrecision(18, 2);
                entity.Property(e => e.Gems).HasPrecision(18, 2);
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