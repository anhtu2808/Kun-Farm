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
        /// Lấy bảng xếp hạng theo số tiền
        /// </summary>
        /// <param name="top">Số lượng top player (mặc định 100)</param>
        /// <returns>Bảng xếp hạng theo tiền</returns>
        [HttpGet("money")]
        public async Task<ActionResult<ApiResponse<LeaderboardResponse>>> GetMoneyLeaderboard([FromQuery] int top = 100)
        {
            try
            {
                if (top <= 0) top = 100;
                if (top > 1000) top = 1000; // Giới hạn tối đa

                var leaderboard = await _leaderboardService.GetMoneyLeaderboardAsync(top);
                _logger.LogInformation("Money leaderboard retrieved with {Count} players", leaderboard.Rankings.Count);
                
                return Ok(ApiResponse<LeaderboardResponse>.Success(leaderboard, "Lấy bảng xếp hạng theo tiền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving money leaderboard");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy bảng xếp hạng theo sức khỏe
        /// </summary>
        /// <param name="top">Số lượng top player (mặc định 100)</param>
        /// <returns>Bảng xếp hạng theo sức khỏe</returns>
        [HttpGet("health")]
        public async Task<ActionResult<ApiResponse<LeaderboardResponse>>> GetHealthLeaderboard([FromQuery] int top = 100)
        {
            try
            {
                if (top <= 0) top = 100;
                if (top > 1000) top = 1000; // Giới hạn tối đa

                var leaderboard = await _leaderboardService.GetHealthLeaderboardAsync(top);
                _logger.LogInformation("Health leaderboard retrieved with {Count} players", leaderboard.Rankings.Count);
                
                return Ok(ApiResponse<LeaderboardResponse>.Success(leaderboard, "Lấy bảng xếp hạng theo sức khỏe thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health leaderboard");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy bảng xếp hạng theo độ đói
        /// </summary>
        /// <param name="top">Số lượng top player (mặc định 100)</param>
        /// <returns>Bảng xếp hạng theo độ đói</returns>
        [HttpGet("hunger")]
        public async Task<ActionResult<ApiResponse<LeaderboardResponse>>> GetHungerLeaderboard([FromQuery] int top = 100)
        {
            try
            {
                if (top <= 0) top = 100;
                if (top > 1000) top = 1000; // Giới hạn tối đa

                var leaderboard = await _leaderboardService.GetHungerLeaderboardAsync(top);
                _logger.LogInformation("Hunger leaderboard retrieved with {Count} players", leaderboard.Rankings.Count);
                
                return Ok(ApiResponse<LeaderboardResponse>.Success(leaderboard, "Lấy bảng xếp hạng theo độ đói thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hunger leaderboard");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy thứ hạng của một player cụ thể theo tiền
        /// </summary>
        /// <param name="userId">ID của player</param>
        /// <returns>Thứ hạng theo tiền</returns>
        [HttpGet("money/rank/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPlayerMoneyRank(int userId)
        {
            try
            {
                var rank = await _leaderboardService.GetPlayerMoneyRankAsync(userId);
                
                if (rank.HasValue)
                {
                    var result = new { UserId = userId, Rank = rank.Value, Category = "Money" };
                    return Ok(ApiResponse<object>.Success(result, "Lấy thứ hạng theo tiền thành công"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy player hoặc player không hoạt động", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving money rank for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy thứ hạng của một player cụ thể theo sức khỏe
        /// </summary>
        /// <param name="userId">ID của player</param>
        /// <returns>Thứ hạng theo sức khỏe</returns>
        [HttpGet("health/rank/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPlayerHealthRank(int userId)
        {
            try
            {
                var rank = await _leaderboardService.GetPlayerHealthRankAsync(userId);
                
                if (rank.HasValue)
                {
                    var result = new { UserId = userId, Rank = rank.Value, Category = "Health" };
                    return Ok(ApiResponse<object>.Success(result, "Lấy thứ hạng theo sức khỏe thành công"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy player hoặc player không hoạt động", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health rank for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy thứ hạng của một player cụ thể theo độ đói
        /// </summary>
        /// <param name="userId">ID của player</param>
        /// <returns>Thứ hạng theo độ đói</returns>
        [HttpGet("hunger/rank/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPlayerHungerRank(int userId)
        {
            try
            {
                var rank = await _leaderboardService.GetPlayerHungerRankAsync(userId);
                
                if (rank.HasValue)
                {
                    var result = new { UserId = userId, Rank = rank.Value, Category = "Hunger" };
                    return Ok(ApiResponse<object>.Success(result, "Lấy thứ hạng theo độ đói thành công"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy player hoặc player không hoạt động", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hunger rank for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy tất cả thứ hạng của một player
        /// </summary>
        /// <param name="userId">ID của player</param>
        /// <returns>Tất cả thứ hạng của player</returns>
        [HttpGet("player/{userId}/ranks")]
        public async Task<ActionResult<ApiResponse<object>>> GetPlayerAllRanks(int userId)
        {
            try
            {
                var moneyRank = await _leaderboardService.GetPlayerMoneyRankAsync(userId);
                var healthRank = await _leaderboardService.GetPlayerHealthRankAsync(userId);
                var hungerRank = await _leaderboardService.GetPlayerHungerRankAsync(userId);

                var result = new
                {
                    UserId = userId,
                    MoneyRank = moneyRank,
                    HealthRank = healthRank,
                    HungerRank = hungerRank,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<object>.Success(result, "Lấy tất cả thứ hạng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all ranks for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
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
