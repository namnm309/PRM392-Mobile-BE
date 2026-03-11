using BAL.DTOs.LinkedAccount;

namespace BAL.Services
{
    public interface ILinkedAccountService
    {
        Task<LinkedAccountsResponse> GetLinkedAccountsAsync(Guid userId);
        Task<LinkedAccountDto> LinkAccountAsync(Guid userId, LinkAccountRequest request);
        Task<bool> UnlinkAccountAsync(Guid userId, string provider);
        Task<Guid?> FindUserByProviderAsync(string provider, string providerUserId);
    }
}
