using BAL.DTOs;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Service xử lý logic đồng bộ user với Clerk
    /// </summary>
    public class UserService : IUserService
    {
        private readonly TechStoreContext _context;

        public UserService(TechStoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Tạo user mới khi nhận event user.created từ Clerk
        /// </summary>
        public async Task<User?> CreateUserAsync(ClerkWebhookDataDto clerkUser)
        {
            if (clerkUser == null || string.IsNullOrEmpty(clerkUser.Id))
            {
                return null;
            }

            // Kiểm tra xem user đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == clerkUser.Id);

            if (existingUser != null)
            {
                // User đã tồn tại, cập nhật thay vì tạo mới
                return await UpdateUserAsync(clerkUser);
            }

            // Lấy email từ email_addresses (lấy email đầu tiên đã verified hoặc primary)
            string email = string.Empty;
            if (clerkUser.EmailAddresses != null && clerkUser.EmailAddresses.Any())
            {
                var verifiedEmail = clerkUser.EmailAddresses
                    .FirstOrDefault(e => e.Verification?.Status == "verified" || 
                                        e.Verification?.Status == "from_oauth_google" ||
                                        e.Verification?.Object == "verification_from_oauth");
                email = verifiedEmail?.EmailAddress ?? clerkUser.EmailAddresses.FirstOrDefault()?.EmailAddress ?? string.Empty;
            }
            
            // Nếu không có email từ email_addresses, thử lấy từ external_accounts
            if (string.IsNullOrEmpty(email) && clerkUser.ExternalAccounts != null && clerkUser.ExternalAccounts.Any())
            {
                var externalAccount = clerkUser.ExternalAccounts.FirstOrDefault();
                email = externalAccount?.EmailAddress ?? string.Empty;
            }
            
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException($"Cannot create user without email. ClerkId: {clerkUser.Id}");
            }

            // Lấy phone number
            string? phoneNumber = null;
            if (clerkUser.PhoneNumbers != null && clerkUser.PhoneNumbers.Any())
            {
                var verifiedPhone = clerkUser.PhoneNumbers
                    .FirstOrDefault(p => p.Verification?.Status == "verified");
                phoneNumber = verifiedPhone?.PhoneNumber ?? clerkUser.PhoneNumbers.First().PhoneNumber;
            }

            // Tạo full name từ first_name và last_name
            string? fullName = null;
            if (!string.IsNullOrEmpty(clerkUser.FirstName) || !string.IsNullOrEmpty(clerkUser.LastName))
            {
                fullName = $"{clerkUser.FirstName} {clerkUser.LastName}".Trim();
            }

            // Convert timestamp từ milliseconds sang DateTime (phải dùng UtcDateTime cho PostgreSQL)
            DateTime createdAt = DateTime.UtcNow;
            if (clerkUser.CreatedAt.HasValue)
            {
                createdAt = DateTimeOffset.FromUnixTimeMilliseconds(clerkUser.CreatedAt.Value).UtcDateTime;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                ClerkId = clerkUser.Id,
                Email = email,
                PhoneNumber = phoneNumber,
                FullName = fullName,
                AvatarUrl = clerkUser.ImageUrl,
                Status = "Active",
                Role = "Customer",
                CreatedAt = createdAt,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                // Log thành công
                System.Diagnostics.Debug.WriteLine($"User created successfully: ClerkId={clerkUser.Id}, DbId={user.Id}, Email={email}");
                
                return user;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Xử lý lỗi database constraint (unique key violation, etc.)
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException(
                    $"Failed to save user to database. ClerkId: {clerkUser.Id}, Email: {email}. " +
                    $"Error: {innerException}", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unexpected error saving user. ClerkId: {clerkUser.Id}, Email: {email}. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật user khi nhận event user.updated từ Clerk
        /// </summary>
        public async Task<User?> UpdateUserAsync(ClerkWebhookDataDto clerkUser)
        {
            if (clerkUser == null || string.IsNullOrEmpty(clerkUser.Id))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == clerkUser.Id);

            if (user == null)
            {
                // User chưa tồn tại, tạo mới
                return await CreateUserAsync(clerkUser);
            }

            // Cập nhật thông tin user
            if (clerkUser.EmailAddresses != null && clerkUser.EmailAddresses.Any())
            {
                var verifiedEmail = clerkUser.EmailAddresses
                    .FirstOrDefault(e => e.Verification?.Status == "verified" || 
                                        e.Verification?.Status == "from_oauth_google" ||
                                        e.Verification?.Object == "verification_from_oauth");
                if (verifiedEmail != null && !string.IsNullOrEmpty(verifiedEmail.EmailAddress))
                {
                    user.Email = verifiedEmail.EmailAddress;
                }
                else
                {
                    var firstEmail = clerkUser.EmailAddresses.FirstOrDefault();
                    if (firstEmail != null && !string.IsNullOrEmpty(firstEmail.EmailAddress))
                    {
                        user.Email = firstEmail.EmailAddress;
                    }
                }
            }
            
            // Nếu không có email từ email_addresses, thử lấy từ external_accounts
            if (string.IsNullOrEmpty(user.Email) && clerkUser.ExternalAccounts != null && clerkUser.ExternalAccounts.Any())
            {
                var externalAccount = clerkUser.ExternalAccounts.FirstOrDefault();
                if (externalAccount != null && !string.IsNullOrEmpty(externalAccount.EmailAddress))
                {
                    user.Email = externalAccount.EmailAddress;
                }
            }

            if (clerkUser.PhoneNumbers != null && clerkUser.PhoneNumbers.Any())
            {
                var verifiedPhone = clerkUser.PhoneNumbers
                    .FirstOrDefault(p => p.Verification?.Status == "verified");
                if (verifiedPhone != null && !string.IsNullOrEmpty(verifiedPhone.PhoneNumber))
                {
                    user.PhoneNumber = verifiedPhone.PhoneNumber;
                }
                else
                {
                    var firstPhone = clerkUser.PhoneNumbers.FirstOrDefault();
                    if (firstPhone != null && !string.IsNullOrEmpty(firstPhone.PhoneNumber))
                    {
                        user.PhoneNumber = firstPhone.PhoneNumber;
                    }
                }
            }

            // Cập nhật full name
            if (!string.IsNullOrEmpty(clerkUser.FirstName) || !string.IsNullOrEmpty(clerkUser.LastName))
            {
                user.FullName = $"{clerkUser.FirstName} {clerkUser.LastName}".Trim();
            }

            // Cập nhật avatar
            if (!string.IsNullOrEmpty(clerkUser.ImageUrl))
            {
                user.AvatarUrl = clerkUser.ImageUrl;
            }

            // Cập nhật timestamp (phải dùng UtcDateTime cho PostgreSQL)
            if (clerkUser.UpdatedAt.HasValue)
            {
                user.UpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(clerkUser.UpdatedAt.Value).UtcDateTime;
            }
            else
            {
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Xóa/Deactivate user khi nhận event user.deleted từ Clerk
        /// </summary>
        public async Task<bool> DeleteUserAsync(string clerkId)
        {
            if (string.IsNullOrEmpty(clerkId))
            {
                return false;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == clerkId);

            if (user == null)
            {
                return false;
            }

            // Đánh dấu user là Inactive thay vì xóa (soft delete)
            user.Status = "Inactive";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Lấy user theo ClerkId (để debug/kiểm tra)
        /// </summary>
        public async Task<User?> GetUserByClerkIdAsync(string clerkId)
        {
            if (string.IsNullOrEmpty(clerkId))
            {
                return null;
            }

            return await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == clerkId);
        }
    }
}
