using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Order repository interface
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByIdWithItemsAsync(Guid id);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> CountByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
