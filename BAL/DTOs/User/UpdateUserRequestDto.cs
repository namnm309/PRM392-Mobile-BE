using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.User
{
    /// <summary>
    /// DTO for updating user information
    /// </summary>
    public class UpdateUserRequestDto
    {
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
        public string? FullName { get; set; }

        [MaxLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string? AvatarUrl { get; set; }

        /// <summary>Date of birth in ISO date format (YYYY-MM-DD). Accepted from mobile as string.</summary>
        [MaxLength(20)]
        public string? DateOfBirth { get; set; }

        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        public string? Gender { get; set; }

        [MaxLength(500, ErrorMessage = "Default address cannot exceed 500 characters")]
        public string? DefaultAddress { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }
    }
}
