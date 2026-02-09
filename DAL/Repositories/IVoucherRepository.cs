using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Voucher repository interface
    /// </summary>
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<IEnumerable<Voucher>> GetAllWithFiltersAsync(string? code = null, string? name = null, bool? isActive = null);
        Task<Voucher?> GetByCodeAsync(string code);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<int> GetUsageCountAsync(Guid voucherId);
        Task<int> GetUsageCountByUserAsync(Guid voucherId, Guid userId);
    }
}
