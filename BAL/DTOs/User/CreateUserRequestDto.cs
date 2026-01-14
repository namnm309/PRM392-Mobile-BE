using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.User
{
    /// <summary>
    /// DTO for creating a new user
    /// </summary>
    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "ClerkId is required")]
        [MaxLength(100, ErrorMessage = "ClerkId cannot exceed 100 characters")]
        public string ClerkId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
        public string? FullName { get; set; }

        [MaxLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string? AvatarUrl { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        public string? Gender { get; set; }

        [MaxLength(500, ErrorMessage = "Default address cannot exceed 500 characters")]
        public string? DefaultAddress { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string Status { get; set; } = "Active";

        [MaxLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
        public string Role { get; set; } = "Customer";
    }
}
