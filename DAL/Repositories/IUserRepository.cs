using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// User-specific repository interface
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByClerkIdAsync(string clerkId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null);
        Task<bool> IsClerkIdExistsAsync(string clerkId, Guid? excludeUserId = null);
    }
}
