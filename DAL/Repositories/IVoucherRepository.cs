using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Voucher repository interface
    /// </summary>
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<int> GetUsageCountAsync(Guid voucherId);
        Task<int> GetUsageCountByUserAsync(Guid voucherId, Guid userId);
    }
}
