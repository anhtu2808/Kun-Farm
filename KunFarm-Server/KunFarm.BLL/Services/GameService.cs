using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KunFarm.BLL.Services
{
    public class GameService : IGameService
    {
        private readonly IPlayerStateRepository _playerStateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFarmStateRepository _farmStateRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IPlayerStateRepository playerStateRepository,
            IUserRepository userRepository,
            IFarmStateRepository farmStateRepository,
            ILogger<GameService> logger)
        {
            _playerStateRepository = playerStateRepository;
            _userRepository = userRepository;
            _farmStateRepository = farmStateRepository;
            _logger = logger;
        }

        public async Task<PlayerStateResponse?> LoadGameAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Loading game state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }

                // Get player state
                var playerState = await _playerStateRepository.GetByUserIdAsync(userId);
                
                if (playerState == null)
                {
                    _logger.LogInformation("No save data found for user: {UserId}, creating default state", userId);
                    
                    // Create default player state
                    var defaultState = new PlayerState
                    {
                        UserId = userId,
                        Money = 0,
                        PosX = 0f,
                        PosY = 0f,
                        PosZ = 0f,
                        LastSaved = DateTime.UtcNow
                    };

                    await _playerStateRepository.CreatePlayerStateAsync(defaultState);
                    return new PlayerStateResponse
                    {
                        UserId = defaultState.UserId,
                        Money = defaultState.Money,
                        PosX = defaultState.PosX,
                        PosY = defaultState.PosY,
                        PosZ = defaultState.PosZ,
                        LastSaved = defaultState.LastSaved
                    };
                }

                _logger.LogInformation("Game state loaded successfully for user: {UserId}", userId);
                return new PlayerStateResponse
                {
                    UserId = playerState.UserId,
                    Money = playerState.Money,
                    PosX = playerState.PosX,
                    PosY = playerState.PosY,
                    PosZ = playerState.PosZ,
                    LastSaved = playerState.LastSaved
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading game state for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> SaveGameAsync(int userId, SaveGameRequest request)
        {
            try
            {
                _logger.LogInformation("Saving game state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return false;
                }

                // Save player state
                var success = await _playerStateRepository.SavePlayerStateAsync(
                    userId, 
                    request.money, 
                    request.posX, 
                    request.posY, 
                    request.posZ);

                if (success)
                {
                    _logger.LogInformation("Game state saved successfully for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to save game state for user: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game state for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<FarmStateResponse?> LoadFarmStateAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Loading farm state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }

                // Get farm state
                var farmState = await _farmStateRepository.GetByUserIdAsync(userId);
                
                if (farmState == null)
                {
                    _logger.LogInformation("No farm data found for user: {UserId}, creating default state", userId);
                    
                    // Create default farm state
                    var defaultFarmState = new FarmState
                    {
                        UserId = userId,
                        TileStatesJson = "[]",
                        PlantsJson = "[]",
                        LastSaved = DateTime.UtcNow
                    };

                    await _farmStateRepository.CreateFarmStateAsync(defaultFarmState);
                    return new FarmStateResponse
                    {
                        UserId = defaultFarmState.UserId,
                        TileStates = new List<DTOs.Response.TileStateResponseData>(),
                        Plants = new List<DTOs.Response.PlantResponseData>(),
                        LastSaved = defaultFarmState.LastSaved
                    };
                }

                // Log raw JSON data for debugging
                _logger.LogInformation("Raw TileStatesJson: {TileStatesJson}", farmState.TileStatesJson);
                _logger.LogInformation("Raw PlantsJson: {PlantsJson}", farmState.PlantsJson);
                
                // Deserialize JSON data with camelCase naming policy
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var tileStates = JsonSerializer.Deserialize<List<DTOs.Response.TileStateResponseData>>(farmState.TileStatesJson, jsonOptions) ?? new List<DTOs.Response.TileStateResponseData>();
                var plants = JsonSerializer.Deserialize<List<DTOs.Response.PlantResponseData>>(farmState.PlantsJson, jsonOptions) ?? new List<DTOs.Response.PlantResponseData>();
                
                _logger.LogInformation("Deserialized {TileCount} tiles and {PlantCount} plants from JSON", tileStates.Count, plants.Count);
                
                // Log first few items for debugging
                if (tileStates.Count > 0)
                {
                    var firstTile = tileStates[0];
                    _logger.LogInformation("First tile: X={X}, Y={Y}, Z={Z}, State={State}", firstTile.X, firstTile.Y, firstTile.Z, firstTile.State);
                }
                if (plants.Count > 0)
                {
                    var firstPlant = plants[0];
                    _logger.LogInformation("First plant: X={X}, Y={Y}, Z={Z}, CropType={CropType}, Stage={Stage}, Mature={Mature}", 
                        firstPlant.X, firstPlant.Y, firstPlant.Z, firstPlant.CropType, firstPlant.CurrentStage, firstPlant.IsMature);
                }

                _logger.LogInformation("Farm state loaded successfully for user: {UserId}", userId);
                return new FarmStateResponse
                {
                    UserId = farmState.UserId,
                    TileStates = tileStates,
                    Plants = plants,
                    LastSaved = farmState.LastSaved
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading farm state for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> SaveFarmStateAsync(int userId, SaveFarmStateRequest request)
        {
            try
            {
                _logger.LogInformation("Saving farm state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return false;
                }

                // Convert request data to JSON with camelCase naming policy
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var tileStatesJson = JsonSerializer.Serialize(request.tileStates, jsonOptions);
                var plantsJson = JsonSerializer.Serialize(request.plants, jsonOptions);
                
                _logger.LogInformation("Serializing {TileCount} tiles and {PlantCount} plants to JSON", request.tileStates?.Count ?? 0, request.plants?.Count ?? 0);

                // Save farm state
                var success = await _farmStateRepository.SaveFarmStateAsync(
                    userId, 
                    tileStatesJson, 
                    plantsJson);

                if (success)
                {
                    _logger.LogInformation("Farm state saved successfully for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to save farm state for user: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving farm state for user: {UserId}", userId);
                return false;
            }
        }
    }
} 