using BAL.DTOs.User;

namespace BAL.Services
{
    /// <summary>
    /// User service interface - Application layer
    /// </summary>
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<UserResponseDto?> GetUserByClerkIdAsync(string clerkId);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
        Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> UpdateLastLoginAsync(string clerkId);
    }
}
