using BAL.DTOs;
using BAL.DTOs.User;
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

        // ============================================
        // API methods (cho UsersController)
        // ============================================

        /// <summary>
        /// Map User entity sang UserResponseDto
        /// </summary>
        private static UserResponseDto MapToDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                ClerkId = user.ClerkId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                DefaultAddress = user.DefaultAddress,
                City = user.City,
                LoyaltyPoints = user.LoyaltyPoints,
                Status = user.Status,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// Lấy user theo ID
        /// </summary>
        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            return MapToDto(user);
        }

        /// <summary>
        /// Lấy user theo ClerkId - API version
        /// </summary>
        public async Task<UserResponseDto?> GetUserByClerkIdAsync(string clerkId)
        {
            if (string.IsNullOrEmpty(clerkId))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == clerkId);

            if (user == null)
            {
                return null;
            }

            return MapToDto(user);
        }

        /// <summary>
        /// Tạo user mới từ API request
        /// </summary>
        public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Kiểm tra xem ClerkId đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.ClerkId == request.ClerkId);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with ClerkId '{request.ClerkId}' already exists");
            }

            // Kiểm tra email đã tồn tại chưa
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                ClerkId = request.ClerkId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FullName = request.FullName,
                AvatarUrl = request.AvatarUrl,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                DefaultAddress = request.DefaultAddress,
                City = request.City,
                Status = request.Status,
                Role = request.Role,
                LoyaltyPoints = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return MapToDto(user);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException(
                    $"Failed to save user to database. ClerkId: {request.ClerkId}, Email: {request.Email}. " +
                    $"Error: {innerException}", dbEx);
            }
        }

        /// <summary>
        /// Cập nhật user từ API request
        /// </summary>
        public async Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            // Cập nhật các field có giá trị
            if (request.PhoneNumber != null)
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            if (request.FullName != null)
            {
                user.FullName = request.FullName;
            }

            if (request.AvatarUrl != null)
            {
                user.AvatarUrl = request.AvatarUrl;
            }

            if (request.DateOfBirth.HasValue)
            {
                user.DateOfBirth = request.DateOfBirth;
            }

            if (request.Gender != null)
            {
                user.Gender = request.Gender;
            }

            if (request.DefaultAddress != null)
            {
                user.DefaultAddress = request.DefaultAddress;
            }

            if (request.City != null)
            {
                user.City = request.City;
            }

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return MapToDto(user);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException(
                    $"Failed to update user in database. UserId: {id}. " +
                    $"Error: {innerException}", dbEx);
            }
        }

        /// <summary>
        /// Xóa user (soft delete) từ API request
        /// </summary>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

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
    }
}
