using BAL.DTOs.LinkedAccount;
using DAL.Models;
using DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public class LinkedAccountService : ILinkedAccountService
    {
        private readonly ILinkedAccountRepository _linkedAccountRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LinkedAccountService> _logger;
        private static readonly string[] SupportedProviders = { "Google", "Facebook", "Apple" };

        public LinkedAccountService(
            ILinkedAccountRepository linkedAccountRepository,
            IUserRepository userRepository,
            ILogger<LinkedAccountService> logger)
        {
            _linkedAccountRepository = linkedAccountRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<LinkedAccountsResponse> GetLinkedAccountsAsync(Guid userId)
        {
            var accounts = await _linkedAccountRepository.GetByUserIdAsync(userId);
            var linkedProviders = accounts.Select(la => la.Provider).ToHashSet();

            return new LinkedAccountsResponse
            {
                Accounts = accounts.Select(MapToDto).ToList(),
                AvailableProviders = SupportedProviders.Select(p => new AvailableProvider
                {
                    Provider = p,
                    IsLinked = linkedProviders.Contains(p)
                }).ToList()
            };
        }

        public async Task<LinkedAccountDto> LinkAccountAsync(Guid userId, LinkAccountRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            if (!SupportedProviders.Contains(request.Provider))
                throw new ArgumentException($"Provider '{request.Provider}' is not supported");

            var existingForUser = await _linkedAccountRepository.GetByUserIdAndProviderAsync(userId, request.Provider);
            if (existingForUser != null)
                throw new InvalidOperationException($"Account already linked with {request.Provider}");

            var existingProvider = await _linkedAccountRepository.GetByProviderUserIdAsync(request.Provider, request.ProviderUserId);
            if (existingProvider != null)
                throw new InvalidOperationException($"This {request.Provider} account is already linked to another user");

            var account = new LinkedAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = request.Provider,
                ProviderUserId = request.ProviderUserId,
                ProviderEmail = request.ProviderEmail,
                ProviderName = request.ProviderName,
                ProviderAvatarUrl = request.ProviderAvatarUrl,
                LinkedAt = DateTime.UtcNow
            };

            await _linkedAccountRepository.AddAsync(account);
            _logger.LogInformation("User {UserId} linked {Provider} account", userId, request.Provider);
            return MapToDto(account);
        }

        public async Task<bool> UnlinkAccountAsync(Guid userId, string provider)
        {
            var account = await _linkedAccountRepository.GetByUserIdAndProviderAsync(userId, provider);
            if (account == null) return false;

            await _linkedAccountRepository.DeleteAsync(account.Id);
            _logger.LogInformation("User {UserId} unlinked {Provider} account", userId, provider);
            return true;
        }

        public async Task<Guid?> FindUserByProviderAsync(string provider, string providerUserId)
        {
            var account = await _linkedAccountRepository.GetByProviderUserIdAsync(provider, providerUserId);
            return account?.UserId;
        }

        private LinkedAccountDto MapToDto(LinkedAccount a) => new()
        {
            Id = a.Id,
            Provider = a.Provider,
            ProviderEmail = a.ProviderEmail,
            ProviderName = a.ProviderName,
            ProviderAvatarUrl = a.ProviderAvatarUrl,
            LinkedAt = a.LinkedAt,
            LastUsedAt = a.LastUsedAt
        };
    }
}
