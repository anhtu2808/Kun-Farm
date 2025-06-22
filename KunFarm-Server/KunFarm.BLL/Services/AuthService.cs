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
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
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
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Tài khoản không tồn tại hoặc đã bị khóa"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Mật khẩu không chính xác"
                    };
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Generate token
                var token = GenerateToken(user.Id, user.Username);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    User = new KunFarm.BLL.DTOs.Response.User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        Level = user.Level,
                        Experience = user.Experience,
                        Coins = user.Coins,
                        Gems = user.Gems,
                        LastLoginAt = user.LastLoginAt
                    },
                    Token = token
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập"
                };
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if username exists
                if (await _userRepository.ExistsByUsernameAsync(request.Username))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Username đã tồn tại"
                    };
                }

                // Check if email exists
                if (await _userRepository.ExistsByEmailAsync(request.Email))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Email đã tồn tại"
                    };
                }

                // Create new user
                var user = new KunFarm.DAL.Entities.User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    DisplayName = string.IsNullOrEmpty(request.DisplayName) ? request.Username : request.DisplayName,
                    IsActive = true,
                    Level = 1,
                    Experience = 0,
                    Coins = 1000,
                    Gems = 10
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // Generate token
                var token = GenerateToken(user.Id, user.Username);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    User = new KunFarm.BLL.DTOs.Response.User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        Level = user.Level,
                        Experience = user.Experience,
                        Coins = user.Coins,
                        Gems = user.Gems,
                        LastLoginAt = user.LastLoginAt
                    },
                    Token = token
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng ký"
                };
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