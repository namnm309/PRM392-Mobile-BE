using DAL.Models;

namespace DAL.Repositories
{
    public interface IPointTransactionRepository : IRepository<PointTransaction>
    {
        Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId, int limit = 20);
        Task<int> GetTotalPointsEarnedAsync(Guid userId);
        Task<int> GetTotalPointsRedeemedAsync(Guid userId);
    }
}
