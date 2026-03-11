using DAL.Models;

namespace DAL.Repositories
{
    public interface IMembershipTierRepository : IRepository<MembershipTier>
    {
        Task<IEnumerable<MembershipTier>> GetAllActiveAsync();
        Task<MembershipTier?> GetTierByPointsAsync(int points);
    }
}
