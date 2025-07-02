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

        [HttpPost("farm/save")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> SaveFarmState([FromBody] SaveFarmStateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid request data", 400));
                }

                var userId = request.userId;
                
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var success = await _gameService.SaveFarmStateAsync(userId, request);
                
                if (success)
                {
                    return Ok(ApiResponse<bool>.Success(true, "Farm state saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.Error("Failed to save farm state", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveFarmState endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }

        [HttpGet("farm/load/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<FarmStateResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<FarmStateResponse>>> LoadFarmState(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var farmState = await _gameService.LoadFarmStateAsync(userId);
                
                if (farmState != null)
                {
                    return Ok(ApiResponse<FarmStateResponse>.Success(farmState, "Farm state loaded successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<FarmStateResponse>.Error("Failed to load farm state", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoadFarmState endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }

        [HttpPost("toolbar/save")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> SaveToolbar([FromBody] SaveToolbarRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid request data", 400));
                }

                var userId = request.userId;
                
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var success = await _gameService.SaveToolbarAsync(userId, request);
                
                if (success)
                {
                    return Ok(ApiResponse<bool>.Success(true, "Toolbar saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.Error("Failed to save toolbar", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveToolbar endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }

        [HttpGet("toolbar/load/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<ToolbarResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<ToolbarResponse>>> LoadToolbar(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<object>.Error("Invalid user ID", 400));
                }

                var toolbar = await _gameService.LoadToolbarAsync(userId);
                
                if (toolbar != null)
                {
                    return Ok(ApiResponse<ToolbarResponse>.Success(toolbar, "Toolbar loaded successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<ToolbarResponse>.Error("Failed to load toolbar", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoadToolbar endpoint");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }
    }
} 