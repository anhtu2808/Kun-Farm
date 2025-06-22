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
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 401)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        [ProducesResponseType(typeof(LoginResponse), 500)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Dữ liệu đầu vào không hợp lệ"
                    });
                }

                var result = await _authService.LoginAsync(request);
                
                if (result.Success)
                {
                    _logger.LogInformation("User {Username} logged in successfully", request.UsernameOrEmail);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for {Username}", request.UsernameOrEmail);
                    return Unauthorized(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Username}", request.UsernameOrEmail);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trên server"
                });
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
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        [ProducesResponseType(typeof(LoginResponse), 500)]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Dữ liệu đầu vào không hợp lệ"
                    });
                }

                var result = await _authService.RegisterAsync(request);
                
                if (result.Success)
                {
                    _logger.LogInformation("User {Username} registered successfully", request.Username);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed registration attempt for {Username}", request.Username);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Username}", request.Username);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trên server"
                });
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
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<object>> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new { valid = false, message = "Có lỗi xảy ra khi xác thực token" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        /// <response code="200">API đang hoạt động bình thường</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult<object> Health()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "KunFarm Auth API"
            });
        }
    }
} 