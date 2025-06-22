using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KunFarm.BLL.DTOs.Request
{
    public class LoginRequest
    {
        /// <summary>
        /// Username hoặc Email của người dùng
        /// </summary>
        /// <example>admin</example>
        [Required(ErrorMessage = "Username hoặc Email là bắt buộc")]
        [Description("Username hoặc Email của người dùng")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu của người dùng
        /// </summary>
        /// <example>admin123</example>
        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
        [Description("Mật khẩu của người dùng (tối thiểu 6 ký tự)")]
        public string Password { get; set; } = string.Empty;
    }
} 