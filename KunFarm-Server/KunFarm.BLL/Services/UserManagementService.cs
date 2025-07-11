using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace KunFarm.BLL.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPlayerStateRepository _playerStateRepository;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUserRepository userRepository, 
            IPlayerStateRepository playerStateRepository,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _playerStateRepository = playerStateRepository;
            _logger = logger;
        }

        public async Task<List<UserListResponse>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userResponses = new List<UserListResponse>();

                foreach (var user in users)
                {
                    var playerState = await _playerStateRepository.GetByUserIdAsync(user.Id);
                    
                    userResponses.Add(new UserListResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        LastLoginAt = user.LastLoginAt,
                        IsActive = user.IsActive,
                        Role = user.Role.ToString(),
                        Money = playerState?.Money ?? 0,
                        Health = playerState?.Health ?? 0,
                        Hunger = playerState?.Hunger ?? 0,
                        LastSaved = playerState?.LastSaved ?? DateTime.MinValue,
                        CreatedAt = user.CreatedAt
                    });
                }

                return userResponses.OrderByDescending(u => u.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserListResponse>();
            }
        }

        public async Task<UserDetailsResponse?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                var playerState = await _playerStateRepository.GetByUserIdAsync(userId);

                return new UserDetailsResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsDeleted = user.IsDeleted,
                    PlayerState = playerState != null ? new PlayerStateInfo
                    {
                        Money = playerState.Money,
                        PosX = playerState.PosX,
                        PosY = playerState.PosY,
                        PosZ = playerState.PosZ,
                        Health = playerState.Health,
                        Hunger = playerState.Hunger,
                        LastSaved = playerState.LastSaved
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                bool hasChanges = false;

                if (!string.IsNullOrEmpty(request.DisplayName) && request.DisplayName != user.DisplayName)
                {
                    user.DisplayName = request.DisplayName;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    if (await _userRepository.ExistsByEmailAsync(request.Email))
                        return false;
                    
                    user.Email = request.Email;
                    hasChanges = true;
                }

                if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
                {
                    user.IsActive = request.IsActive.Value;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(request.Role) && Enum.TryParse<Role>(request.Role, out var role) && role != user.Role)
                {
                    user.Role = role;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdatePlayerStateAsync(int userId, UpdatePlayerStateRequest request)
        {
            try
            {
                var playerState = await _playerStateRepository.GetByUserIdAsync(userId);
                if (playerState == null) return false;

                bool hasChanges = false;

                if (request.Money.HasValue && request.Money.Value != playerState.Money)
                {
                    playerState.Money = request.Money.Value;
                    hasChanges = true;
                }

                if (request.Health.HasValue && request.Health.Value != playerState.Health)
                {
                    playerState.Health = Math.Max(0, Math.Min(100, request.Health.Value));
                    hasChanges = true;
                }

                if (request.Hunger.HasValue && request.Hunger.Value != playerState.Hunger)
                {
                    playerState.Hunger = Math.Max(0, Math.Min(100, request.Hunger.Value));
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    playerState.LastSaved = DateTime.UtcNow;
                    await _playerStateRepository.SavePlayerStateAsync(
                        userId, 
                        playerState.Money, 
                        playerState.PosX, 
                        playerState.PosY, 
                        playerState.PosZ,
                        playerState.Health, 
                        playerState.Hunger);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player state: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                user.IsDeleted = true;
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserListResponse>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllUsersAsync();

                var users = await _userRepository.GetAllAsync();
                var filteredUsers = users.Where(u => 
                    u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                );

                var userResponses = new List<UserListResponse>();

                foreach (var user in filteredUsers)
                {
                    var playerState = await _playerStateRepository.GetByUserIdAsync(user.Id);
                    
                    userResponses.Add(new UserListResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        LastLoginAt = user.LastLoginAt,
                        IsActive = user.IsActive,
                        Role = user.Role.ToString(),
                        Money = playerState?.Money ?? 0,
                        Health = playerState?.Health ?? 0,
                        Hunger = playerState?.Hunger ?? 0,
                        LastSaved = playerState?.LastSaved ?? DateTime.MinValue,
                        CreatedAt = user.CreatedAt
                    });
                }

                return userResponses.OrderByDescending(u => u.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                return new List<UserListResponse>();
            }
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return users.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total users count");
                return 0;
            }
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return users.Count(u => u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users count");
                return 0;
            }
        }
    }
} 