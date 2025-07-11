using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using User = KunFarm.DAL.Entities.User;

namespace KunFarm.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPlayerStateRepository _playerStateRepository;
        private readonly IConfiguration _configuration;
        private readonly IRegularShopSlotService _regularShopSlotService;
        private readonly IInventorySlotService _inventoryService;

        public AuthService(IUserRepository userRepository, IPlayerStateRepository playerStateRepository, IConfiguration configuration, IRegularShopSlotService regularShopSlotService, IInventorySlotService inventorySlotService)
        {
            _userRepository = userRepository;
            _playerStateRepository = playerStateRepository;
            _configuration = configuration;
            _regularShopSlotService = regularShopSlotService;
            _inventoryService = inventorySlotService;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                // Tìm user bằng username hoặc email
                User? user = null;
                
                if (request.UsernameOrEmail.Contains("@"))
                {
                    user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail);
                }
                else
                {
                    user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);
                }

                if (user == null || !user.IsActive)
                {
                    return null;
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return null;
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Generate token
                var token = GenerateToken(user.Id, user.Username);

                return new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        LastLoginAt = user.LastLoginAt,
                        Role = user.Role.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if username exists
                if (await _userRepository.ExistsByUsernameAsync(request.Username))
                {
                    return null;
                }

                // Check if email exists
                if (await _userRepository.ExistsByEmailAsync(request.Email))
                {
                    return null;
                }

                // Create new user
                var user = new KunFarm.DAL.Entities.User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    DisplayName = string.IsNullOrEmpty(request.DisplayName) ? request.Username : request.DisplayName,
                    IsActive = true
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // Create initial PlayerState for new user
                var playerState = new PlayerState
                {
                    UserId = user.Id,
                    Money = 1000, // Starting money
                    PosX = 0f,
                    PosY = 0f,
                    PosZ = 0f,
                    Health = 100f, // Starting health
                    Hunger = 100f, // Starting hunger
                    LastSaved = DateTime.UtcNow
                };

                await _playerStateRepository.CreatePlayerStateAsync(playerState);
                await _regularShopSlotService.CreatePlayerSlot(user.Id);
                await _inventoryService.InitInventory(user.Id);
                // Generate token
                var token = GenerateToken(user.Id, user.Username);

                return new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        LastLoginAt = user.LastLoginAt,
                        Role = user.Role.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateToken(int userId, string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? "sD1IIRal7ci9wDkdJSUKilPtWvi7LDDC5GlgjXNArl8=";
            var issuer = jwtSettings["Issuer"] ?? "KunFarm";
            var audience = jwtSettings["Audience"] ?? "KunFarmClient";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Token expires in 60 minutes
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var secretKey = jwtSettings["Key"] ?? "sD1IIRal7ci9wDkdJSUKilPtWvi7LDDC5GlgjXNArl8=";

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 