using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KunFarm.Presentation.Controllers
{
    [ApiController]
    [Route("/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Kết quả đăng nhập và JWT token</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="401">Thông tin đăng nhập không chính xác</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Dữ liệu đầu vào không hợp lệ", 400));
                }

                var result = await _authService.LoginAsync(request);
                
                if (result != null)
                {
                    _logger.LogInformation("User {Username} logged in successfully", request.UsernameOrEmail);
                    return Ok(ApiResponse<AuthResponse>.Success(result, "Đăng nhập thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for {Username}", request.UsernameOrEmail);
                    return Unauthorized(ApiResponse<object>.Error("Thông tin đăng nhập không chính xác", 401));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Username}", request.UsernameOrEmail);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Kết quả đăng ký và JWT token</returns>
        /// <response code="200">Đăng ký thành công</response>
        /// <response code="400">Username/Email đã tồn tại hoặc dữ liệu không hợp lệ</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Dữ liệu đầu vào không hợp lệ", 400));
                }

                var result = await _authService.RegisterAsync(request);
                
                if (result != null)
                {
                    _logger.LogInformation("User {Username} registered successfully", request.Username);
                    return Ok(ApiResponse<AuthResponse>.Success(result, "Đăng ký thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed registration attempt for {Username}", request.Username);
                    return BadRequest(ApiResponse<object>.Error("Username hoặc Email đã tồn tại", 400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Username}", request.Username);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Kết quả xác thực</returns>
        /// <response code="200">Token hợp lệ</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                return Ok(ApiResponse<object>.Success(new { valid = isValid }, "Token validation completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra khi xác thực token", 500));
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        /// <response code="200">API đang hoạt động bình thường</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public ActionResult<ApiResponse<object>> Health()
        {
            return Ok(ApiResponse<object>.Success(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "KunFarm Auth API"
            }, "API is healthy"));
        }
    }
} 