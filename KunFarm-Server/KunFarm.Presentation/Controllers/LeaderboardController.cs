using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KunFarm.Presentation.Controllers
{
    [ApiController]
    [Route("/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(
            ILeaderboardService leaderboardService,
            ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get money leaderboard rankings
        /// </summary>
        /// <param name="top">Number of top players (default 100)</param>
        /// <returns>Money leaderboard rankings</returns>
        [HttpGet("money")]
        public async Task<ActionResult<ApiResponse<LeaderboardResponse>>> GetMoneyLeaderboard([FromQuery] int top = 100)
        {
            try
            {
                if (top <= 0) top = 100;
                if (top > 1000) top = 1000; // Maximum limit

                var leaderboard = await _leaderboardService.GetMoneyLeaderboardAsync(top);
                _logger.LogInformation("Money leaderboard retrieved with {Count} players", leaderboard.Rankings.Count);
                
                return Ok(ApiResponse<LeaderboardResponse>.Success(leaderboard, "Money leaderboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving money leaderboard");
                return StatusCode(500, ApiResponse<object>.Error("Server error occurred", 500));
            }
        }

        /// <summary>
        /// Get specific player's money rank
        /// </summary>
        /// <param name="userId">Player ID</param>
        /// <returns>Player's money rank</returns>
        [HttpGet("money/rank/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPlayerMoneyRank(int userId)
        {
            try
            {
                var rank = await _leaderboardService.GetPlayerMoneyRankAsync(userId);
                
                if (rank.HasValue)
                {
                    var result = new { UserId = userId, Rank = rank.Value, Category = "Money" };
                    return Ok(ApiResponse<object>.Success(result, "Money rank retrieved successfully"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.Error("Player not found or inactive", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving money rank for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Server error occurred", 500));
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet("health-check")]
        public ActionResult<ApiResponse<object>> HealthCheck()
        {
            return Ok(ApiResponse<object>.Success(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "KunFarm Leaderboard API"
            }, "Leaderboard API is healthy"));
        }
    }
}
