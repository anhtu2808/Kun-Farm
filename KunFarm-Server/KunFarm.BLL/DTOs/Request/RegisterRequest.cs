using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KunFarm.BLL.DTOs.Request
{
    public class RegisterRequest
    {
        /// <summary>
        /// Tên đăng nhập duy nhất
        /// </summary>
        /// <example>newuser</example>
        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, ErrorMessage = "Username không được quá 50 ký tự")]
        [Description("Tên đăng nhập duy nhất (tối đa 50 ký tự)")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ email duy nhất
        /// </summary>
        /// <example>newuser@example.com</example>
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [Description("Địa chỉ email hợp lệ (tối đa 100 ký tự)")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu cho tài khoản
        /// </summary>
        /// <example>password123</example>
        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
        [StringLength(100, ErrorMessage = "Password không được quá 100 ký tự")]
        [Description("Mật khẩu (6-100 ký tự)")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Tên hiển thị (tùy chọn)
        /// </summary>
        /// <example>New User</example>
        [StringLength(100, ErrorMessage = "DisplayName không được quá 100 ký tự")]
        [Description("Tên hiển thị trong game (tùy chọn, tối đa 100 ký tự)")]
        public string DisplayName { get; set; } = string.Empty;
    }
} 