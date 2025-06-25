using KunFarm.DAL.Data;
using KunFarm.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KunFarm.BLL.Services
{
    public class DatabaseSeederService
    {
        private readonly KunFarmDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(
            KunFarmDbContext context, 
            IConfiguration configuration,
            ILogger<DatabaseSeederService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Kiểm tra xem admin account đã tồn tại chưa
                var adminConfig = _configuration.GetSection("AdminAccount");
                var adminUsername = adminConfig["Username"];
                var adminEmail = adminConfig["Email"];

                if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminEmail))
                {
                    _logger.LogWarning("Admin account configuration is missing");
                    return;
                }

                var existingAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == adminUsername || u.Email == adminEmail);

                if (existingAdmin == null)
                {
                    _logger.LogInformation("Creating admin account from configuration");

                    var adminUser = new User
                    {
                        Username = adminUsername,
                        Email = adminEmail,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig["Password"] ?? "admin"),
                        DisplayName = adminConfig["DisplayName"] ?? "Administrator",
                        IsActive = true,
                        Role = Role.ADMIN,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    // Create PlayerState for admin (optional, in case admin wants to play)
                    var adminPlayerState = new PlayerState
                    {
                        UserId = adminUser.Id,
                        Money = int.Parse(adminConfig["Money"] ?? "10000"), // Admin gets more money
                        PosX = 0f,
                        PosY = 0f,
                        PosZ = 0f,
                        LastSaved = DateTime.UtcNow
                    };

                    _context.PlayerStates.Add(adminPlayerState);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Admin account created successfully: {Username}", adminUsername);
                }
                else
                {
                    _logger.LogInformation("Admin account already exists: {Username}", adminUsername);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding admin account");
                throw;
            }
        }
    }
} 