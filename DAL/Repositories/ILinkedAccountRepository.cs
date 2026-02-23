using DAL.Models;

namespace DAL.Repositories
{
    public interface ILinkedAccountRepository : IRepository<LinkedAccount>
    {
        Task<IEnumerable<LinkedAccount>> GetByUserIdAsync(Guid userId);
        Task<LinkedAccount?> GetByUserIdAndProviderAsync(Guid userId, string provider);
        Task<LinkedAccount?> GetByProviderUserIdAsync(string provider, string providerUserId);
    }
}
