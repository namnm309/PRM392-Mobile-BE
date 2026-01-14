using BAL.DTOs;
using DAL.Models;

namespace BAL.Services
{
    /// <summary>
    /// Interface cho UserService - xử lý logic đồng bộ user với Clerk
    /// </summary>
    public interface IUserService
    {
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

        /// <summary>
        /// Lấy user theo ClerkId (để debug/kiểm tra)
        /// </summary>
        Task<User?> GetUserByClerkIdAsync(string clerkId);
    }
}
