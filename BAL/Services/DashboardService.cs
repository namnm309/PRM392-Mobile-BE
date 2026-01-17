using BAL.DTOs.Dashboard;
using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Dashboard service implementation
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly TechStoreContext _context;

        public DashboardService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            IVoucherUsageRepository voucherUsageRepository,
            TechStoreContext context)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _voucherUsageRepository = voucherUsageRepository;
            _context = context;
        }

        public async Task<DashboardOverviewResponseDto> GetOverviewAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync(startDate, endDate);
            var totalOrders = await _orderRepository.CountByStatusAsync("SUCCESS", startDate, endDate) +
                             await _orderRepository.CountByStatusAsync("Pending", startDate, endDate) +
                             await _orderRepository.CountByStatusAsync("Processing", startDate, endDate) +
                             await _orderRepository.CountByStatusAsync("Shipped", startDate, endDate) +
                             await _orderRepository.CountByStatusAsync("Delivered", startDate, endDate) +
                             await _orderRepository.CountByStatusAsync("Cancelled", startDate, endDate);
            var successOrders = await _orderRepository.CountByStatusAsync("SUCCESS", startDate, endDate);
            var cancelledOrders = await _orderRepository.CountByStatusAsync("Cancelled", startDate, endDate);
            
            // Count new users in date range
            var newUsers = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .CountAsync();

            return new DashboardOverviewResponseDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                SuccessOrders = successOrders,
                CancelledOrders = cancelledOrders,
                NewUsers = newUsers,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<IEnumerable<TopProductResponseDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topN = 10)
        {
            var orderItems = await _orderItemRepository.FindAsync(oi => 
                oi.Status == "SUCCESS" && 
                oi.CreatedAt >= startDate && 
                oi.CreatedAt <= endDate);

            var productStats = orderItems
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductResponseDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name ?? "Unknown",
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                    OrderCount = g.Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(topN)
                .ToList();

            return productStats;
        }

        public async Task<IEnumerable<OrdersSeriesResponseDto>> GetOrdersSeriesAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
        {
            var orders = await _orderRepository.GetByDateRangeAsync(startDate, endDate);
            var successOrders = orders.Where(o => o.Status == "SUCCESS").ToList();

            IEnumerable<OrdersSeriesResponseDto> series;

            switch (groupBy.ToLower())
            {
                case "week":
                    series = successOrders
                        .GroupBy(o => GetWeekKey(o.CreatedAt))
                        .Select(g => new OrdersSeriesResponseDto
                        {
                            Period = g.Key,
                            OrderCount = g.Count(),
                            Revenue = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(s => s.Period);
                    break;

                case "month":
                    series = successOrders
                        .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                        .Select(g => new OrdersSeriesResponseDto
                        {
                            Period = g.Key,
                            OrderCount = g.Count(),
                            Revenue = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(s => s.Period);
                    break;

                default: // day
                    series = successOrders
                        .GroupBy(o => o.CreatedAt.Date.ToString("yyyy-MM-dd"))
                        .Select(g => new OrdersSeriesResponseDto
                        {
                            Period = g.Key,
                            OrderCount = g.Count(),
                            Revenue = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(s => s.Period);
                    break;
            }

            return series;
        }

        public async Task<IEnumerable<VoucherUsageResponseDto>> GetVoucherUsageAsync(DateTime startDate, DateTime endDate)
        {
            // Get all voucher usages in date range
            var allUsages = await _context.VoucherUsages
                .Include(vu => vu.Voucher)
                .Where(vu => vu.UsedAt >= startDate && vu.UsedAt <= endDate)
                .ToListAsync();
            
            // Group by voucher
            var voucherGroups = allUsages.GroupBy(u => u.VoucherId);
            
            var results = new List<VoucherUsageResponseDto>();

            foreach (var group in voucherGroups)
            {
                var voucher = group.First().Voucher;
                if (voucher == null) continue;

                var usageCount = group.Count();
                var totalDiscount = group.Sum(u => u.DiscountAmount);
                var usagePercentage = voucher.TotalUsageLimit > 0 
                    ? (decimal)usageCount / voucher.TotalUsageLimit * 100 
                    : 0;

                results.Add(new VoucherUsageResponseDto
                {
                    VoucherId = voucher.Id,
                    VoucherCode = voucher.Code,
                    UsageCount = usageCount,
                    TotalDiscountAmount = totalDiscount,
                    TotalUsageLimit = voucher.TotalUsageLimit,
                    UsagePercentage = usagePercentage
                });
            }

            return results.OrderByDescending(r => r.UsageCount);
        }

        private static string GetWeekKey(DateTime date)
        {
            var startOfYear = new DateTime(date.Year, 1, 1);
            var weekNumber = ((date - startOfYear).Days / 7) + 1;
            return $"{date.Year}-W{weekNumber:D2}";
        }
    }
}
