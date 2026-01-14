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
        Task<Order?> GetByIdWithFullDetailsAsync(Guid id);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<(IEnumerable<Order> Orders, int TotalCount)> SearchByOrderIdAsync(string orderIdSearch, int pageNumber, int pageSize);
        Task<(IEnumerable<Order> Orders, int TotalCount)> SearchByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> CountByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
