
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class LogoutRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}