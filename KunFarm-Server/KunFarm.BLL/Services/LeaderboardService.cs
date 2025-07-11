using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace KunFarm.BLL.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPlayerStateRepository _playerStateRepository;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(
            IUserRepository userRepository,
            IPlayerStateRepository playerStateRepository,
            ILogger<LeaderboardService> logger)
        {
            _userRepository = userRepository;
            _playerStateRepository = playerStateRepository;
            _logger = logger;
        }

        public async Task<LeaderboardResponse> GetMoneyLeaderboardAsync(int top = 100)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var activeUsers = users.Where(u => u.IsActive && !u.IsDeleted).ToList();

                var leaderboardEntries = new List<LeaderboardEntry>();

                foreach (var user in activeUsers)
                {
                    var playerState = await _playerStateRepository.GetByUserIdAsync(user.Id);
                    if (playerState != null)
                    {
                        leaderboardEntries.Add(new LeaderboardEntry
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            DisplayName = user.DisplayName,
                            Money = playerState.Money,
                            LastSaved = playerState.LastSaved,
                            IsActive = user.IsActive
                        });
                    }
                }

                // Sort by money descending and take top N
                var sortedEntries = leaderboardEntries
                    .OrderByDescending(e => e.Money)
                    .Take(top)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < sortedEntries.Count; i++)
                {
                    sortedEntries[i].Rank = i + 1;
                }

                return new LeaderboardResponse
                {
                    Rankings = sortedEntries,
                    TotalPlayers = leaderboardEntries.Count,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating money leaderboard");
                return new LeaderboardResponse { GeneratedAt = DateTime.UtcNow };
            }
        }

        public async Task<int?> GetPlayerMoneyRankAsync(int userId)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var activeUsers = users.Where(u => u.IsActive && !u.IsDeleted).ToList();

                var userMoney = new List<(int UserId, int Money)>();

                foreach (var user in activeUsers)
                {
                    var playerState = await _playerStateRepository.GetByUserIdAsync(user.Id);
                    if (playerState != null)
                    {
                        userMoney.Add((user.Id, playerState.Money));
                    }
                }

                // Sort by money descending
                var sortedByMoney = userMoney
                    .OrderByDescending(x => x.Money)
                    .ToList();

                // Find the rank of the specified user
                for (int i = 0; i < sortedByMoney.Count; i++)
                {
                    if (sortedByMoney[i].UserId == userId)
                    {
                        return i + 1; // Rank is 1-based
                    }
                }

                return null; // User not found or not active
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player money rank for user: {UserId}", userId);
                return null;
            }
        }
    }
} 