using BAL.DTOs.Dashboard;

namespace BAL.Services
{
    /// <summary>
    /// Dashboard service interface
    /// </summary>
    public interface IDashboardService
    {
        Task<DashboardOverviewResponseDto> GetOverviewAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TopProductResponseDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topN = 10);
        Task<IEnumerable<OrdersSeriesResponseDto>> GetOrdersSeriesAsync(DateTime startDate, DateTime endDate, string groupBy = "day");
        Task<IEnumerable<VoucherUsageResponseDto>> GetVoucherUsageAsync(DateTime startDate, DateTime endDate);
    }
}
