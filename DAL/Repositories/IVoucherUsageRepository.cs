using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// VoucherUsage repository interface
    /// </summary>
    public interface IVoucherUsageRepository : IRepository<VoucherUsage>
    {
        Task<IEnumerable<VoucherUsage>> GetByVoucherIdAsync(Guid voucherId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<VoucherUsage>> GetByUserIdAsync(Guid userId);
    }
}
