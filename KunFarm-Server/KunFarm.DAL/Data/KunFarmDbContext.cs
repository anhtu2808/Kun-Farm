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
        public DbSet<FarmState> FarmStates { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<RegularShopSlot> RegularShopSlots { get; set; }
        public DbSet<InventorySlot> InventorySlots { get; set; }
        public DbSet<PlayerRegularShopSlot> PlayerRegularShopSlots { get; set; }
        public DbSet<OnlineShopSlot> OnlineShopSlots { get; set; }

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

                entity.HasMany(p => p.InventorySlots)
                    .WithOne(s => s.PlayerState)
                    .HasForeignKey(s => s.PlayerStateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FarmState entity configuration
            modelBuilder.Entity<FarmState>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => e.UserId);
                
                entity.Property(e => e.LastSaved)
                      .HasDefaultValue(DateTime.UtcNow);
                      
                // One-to-many relationship with User
                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Item>()
                        .HasOne(i => i.RegularShop)
                        .WithOne(rs => rs.Item)
                        .HasForeignKey<RegularShopSlot>(rs => rs.ItemId);

            modelBuilder.Entity<InventorySlot>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<InventorySlot>()
                .HasOne(s => s.PlayerState)
                .WithMany(p => p.InventorySlots)
                .HasForeignKey(s => s.PlayerStateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventorySlot>()
                .HasOne(s => s.Item)
                .WithMany(i => i.InventorySlots)
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<PlayerRegularShopSlot>(entity =>
            {
                // Khóa chính gồm cả hai FK
                entity.HasKey(prs => new { prs.PlayerStateId, prs.RegularShopSlotId });

                // Quan hệ tới PlayerState
                entity.HasOne(prs => prs.PlayerState)
                      .WithMany(ps => ps.PlayerRegularShopSlots)
                      .HasForeignKey(prs => prs.PlayerStateId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ tới RegularShopSlot
                entity.HasOne(prs => prs.RegularShopSlot)
                      .WithMany(rs => rs.PlayerRegularShopSlots)
                      .HasForeignKey(prs => prs.RegularShopSlotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OnlineShopSlot>(entity =>
            {
                // Khóa chính (nếu BaseEntity chưa định nghĩa)
                entity.HasKey(o => o.Id);

                // Seller (PlayerState → OnlineShopSlot: 1–n)
                entity.HasOne(o => o.Seller)
                      .WithMany(p => p.SellingOnlineShopSlots)
                      .HasForeignKey(o => o.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Buyer (PlayerState → OnlineShopSlot: 1–n)
                entity.HasOne(o => o.Buyer)
                      .WithMany(p => p.BuyingOnlineShopSlots)
                      .HasForeignKey(o => o.BuyerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Item (Item → OnlineShopSlot: 1–n)
                entity.HasOne(o => o.Item)
                      .WithMany(i => i.OnlineShopSlots)
                      .HasForeignKey(o => o.ItemId)
                      .OnDelete(DeleteBehavior.Cascade);
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