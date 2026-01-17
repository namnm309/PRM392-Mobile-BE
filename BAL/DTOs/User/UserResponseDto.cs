namespace BAL.DTOs.User
{
    /// <summary>
    /// User response DTO
    /// </summary>
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string ClerkId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? DefaultAddress { get; set; }
        public string? City { get; set; }
        public int LoyaltyPoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
