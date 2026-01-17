using BAL.DTOs;
using BAL.DTOs.User;
using DAL.Models;

namespace BAL.Services
{
    /// <summary>
    /// Interface cho UserService - xử lý logic đồng bộ user với Clerk và API operations
    /// </summary>
    public interface IUserService
    {
        // ============================================
        // Webhook methods (từ Clerk)
        // ============================================
        
        /// <summary>
        /// Tạo user mới khi nhận event user.created từ Clerk
        /// </summary>
        Task<User?> CreateUserAsync(ClerkWebhookDataDto clerkUser);

        /// <summary>
        /// Cập nhật user khi nhận event user.updated từ Clerk
        /// </summary>
        Task<User?> UpdateUserAsync(ClerkWebhookDataDto clerkUser);

        /// <summary>
        /// Xóa/Deactivate user khi nhận event user.deleted từ Clerk
        /// </summary>
        Task<bool> DeleteUserAsync(string clerkId);

        // ============================================
        // API methods (cho UsersController)
        // ============================================
        
        /// <summary>
        /// Lấy user theo ID
        /// </summary>
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Lấy user theo ClerkId
        /// </summary>
        Task<UserResponseDto?> GetUserByClerkIdAsync(string clerkId);

        /// <summary>
        /// Tạo user mới từ API request
        /// </summary>
        Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);

        /// <summary>
        /// Cập nhật user từ API request
        /// </summary>
        Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserRequestDto request);

        /// <summary>
        /// Xóa user (soft delete) từ API request
        /// </summary>
        Task<bool> DeleteUserAsync(Guid id);
    }
}
