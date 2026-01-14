using BAL.DTOs.User;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// User service implementation - Contains business logic
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserResponseDto?> GetUserByClerkIdAsync(string clerkId)
        {
            var user = await _userRepository.GetByClerkIdAsync(clerkId);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
        {
            // Business rule: Check if ClerkId already exists
            if (await _userRepository.IsClerkIdExistsAsync(request.ClerkId))
            {
                throw new InvalidOperationException($"User with ClerkId '{request.ClerkId}' already exists");
            }

            // Business rule: Check if Email already exists
            if (await _userRepository.IsEmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");
            }

            // Business rule: Validate status
            var validStatuses = new[] { "Active", "Inactive", "Banned" };
            if (!validStatuses.Contains(request.Status))
            {
                throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            // Business rule: Validate role
            var validRoles = new[] { "Customer", "Admin", "Staff" };
            if (!validRoles.Contains(request.Role))
            {
                throw new ArgumentException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");
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
                LoyaltyPoints = 0, // New users start with 0 points
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Business rule: Only update provided fields
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

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            // Business rule: Soft delete - set status to Inactive instead of hard delete
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Status = "Inactive";
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(string clerkId)
        {
            var user = await _userRepository.GetByClerkIdAsync(clerkId);
            if (user == null)
            {
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

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
    }
}
