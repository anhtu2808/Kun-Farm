using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KunFarm.Presentation.Controllers
{
    [ApiController]
    [Route("/game")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("save")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> SaveGame([FromBody] SaveGameRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid request data", 400));
                }

                // For now, we'll get userId from the request body
                // In a real app, you'd get this from JWT token or session
                var userId = request.userId;
                
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var success = await _gameService.SaveGameAsync(userId, request);
                
                if (success)
                {
                    return Ok(ApiResponse<bool>.Success(true, "Game saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.Error("Failed to save game state", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveGame endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }

        [HttpGet("load/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PlayerStateResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<PlayerStateResponse>>> LoadGame(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var playerState = await _gameService.LoadGameAsync(userId);
                
                if (playerState != null)
                {
                    return Ok(ApiResponse<PlayerStateResponse>.Success(playerState, "Game state loaded successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<PlayerStateResponse>.Error("Failed to load game state", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoadGame endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }
    }
} 