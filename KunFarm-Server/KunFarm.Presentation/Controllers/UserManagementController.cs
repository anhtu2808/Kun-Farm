using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KunFarm.Presentation.Controllers
{
    [ApiController]
    [Route("/admin/user-management")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserManagementService userManagementService,
            ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        /// <returns>Danh sách người dùng</returns>
        /// <response code="200">Lấy danh sách thành công</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserListResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<UserListResponse>>>> GetAllUsers()
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                _logger.LogInformation("Retrieved {UserCount} users", users.Count);
                return Ok(ApiResponse<List<UserListResponse>>.Success(users, "Lấy danh sách người dùng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thông tin chi tiết người dùng</returns>
        /// <response code="200">Lấy thông tin thành công</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<UserDetailsResponse>>> GetUserById(int userId)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy người dùng", 404));
                }

                _logger.LogInformation("Retrieved user details for: {UserId}", userId);
                return Ok(ApiResponse<UserDetailsResponse>.Success(user, "Lấy thông tin người dùng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Dữ liệu đầu vào không hợp lệ", 400));
                }

                var success = await _userManagementService.UpdateUserAsync(userId, request);
                
                if (success)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", userId);
                    return Ok(ApiResponse<object>.Success(null, "Cập nhật người dùng thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy người dùng hoặc email đã tồn tại", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái game của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="request">Thông tin trạng thái game</param>
        /// <returns>Kết quả cập nhật</returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpPut("{userId}/player-state")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> UpdatePlayerState(int userId, [FromBody] UpdatePlayerStateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Error("Dữ liệu đầu vào không hợp lệ", 400));
                }

                var success = await _userManagementService.UpdatePlayerStateAsync(userId, request);
                
                if (success)
                {
                    _logger.LogInformation("Player state updated successfully: {UserId}", userId);
                    return Ok(ApiResponse<object>.Success(null, "Cập nhật trạng thái game thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed to update player state: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy trạng thái game của người dùng", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player state: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Xóa người dùng (soft delete)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả xóa</returns>
        /// <response code="200">Xóa thành công</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int userId)
        {
            try
            {
                var success = await _userManagementService.DeleteUserAsync(userId);
                
                if (success)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                    return Ok(ApiResponse<object>.Success(null, "Xóa người dùng thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy người dùng", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Kích hoạt người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả kích hoạt</returns>
        /// <response code="200">Kích hoạt thành công</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("{userId}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> ActivateUser(int userId)
        {
            try
            {
                var success = await _userManagementService.ActivateUserAsync(userId);
                
                if (success)
                {
                    _logger.LogInformation("User activated successfully: {UserId}", userId);
                    return Ok(ApiResponse<object>.Success(null, "Kích hoạt người dùng thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed to activate user: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy người dùng", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Vô hiệu hóa người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả vô hiệu hóa</returns>
        /// <response code="200">Vô hiệu hóa thành công</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("{userId}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int userId)
        {
            try
            {
                var success = await _userManagementService.DeactivateUserAsync(userId);
                
                if (success)
                {
                    _logger.LogInformation("User deactivated successfully: {UserId}", userId);
                    return Ok(ApiResponse<object>.Success(null, "Vô hiệu hóa người dùng thành công"));
                }
                else
                {
                    _logger.LogWarning("Failed to deactivate user: {UserId}", userId);
                    return NotFound(ApiResponse<object>.Error("Không tìm thấy người dùng", 404));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Tìm kiếm người dùng
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách người dùng phù hợp</returns>
        /// <response code="200">Tìm kiếm thành công</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<UserListResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<UserListResponse>>>> SearchUsers([FromQuery] string searchTerm)
        {
            try
            {
                var users = await _userManagementService.SearchUsersAsync(searchTerm ?? "");
                _logger.LogInformation("Search completed with {UserCount} results for term: {SearchTerm}", users.Count, searchTerm);
                return Ok(ApiResponse<List<UserListResponse>>.Success(users, "Tìm kiếm người dùng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

        /// <summary>
        /// Lấy thống kê người dùng
        /// </summary>
        /// <returns>Thống kê người dùng</returns>
        /// <response code="200">Lấy thống kê thành công</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> GetUserStats()
        {
            try
            {
                var totalUsers = await _userManagementService.GetTotalUsersCountAsync();
                var activeUsers = await _userManagementService.GetActiveUsersCountAsync();

                var stats = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("User stats retrieved: Total={Total}, Active={Active}", totalUsers, activeUsers);
                return Ok(ApiResponse<object>.Success(stats, "Lấy thống kê người dùng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user stats");
                return StatusCode(500, ApiResponse<object>.Error("Có lỗi xảy ra trên server", 500));
            }
        }

		[HttpPut("{userId}/money")]
		[ProducesResponseType(typeof(ApiResponse<object>), 200)]
		[ProducesResponseType(typeof(ApiResponse<object>), 404)]
		[ProducesResponseType(typeof(ApiResponse<object>), 500)]
		public async Task<ActionResult<ApiResponse<object>>> UpdateMoney(int userId, [FromBody] UpdateMoneyRequest req)
		{
			try
			{
				var success = await _userManagementService.UpdateUserMoneyAsync(userId, req.Money);
				if (!success)
					return NotFound(ApiResponse<object>.Error("User not found", 404));

				return Ok(ApiResponse<object>.Success(null, "Money updated successfully"));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating money for user {UserId}", userId);
				return StatusCode(500, ApiResponse<object>.Error("Server error", 500));
			}
		}
	}
} 