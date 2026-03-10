using DAL.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TechStoreController.Services
{
    /// <summary>
    /// Background service to auto-cancel orders that are pending payment for more than 24 hours
    /// </summary>
    public class OrderExpiryScannerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderExpiryScannerService> _logger;
        private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(30); // Scan every 30 minutes
        private readonly TimeSpan _expiryWindow = TimeSpan.FromHours(24); // 24 hours expiry

        public OrderExpiryScannerService(
            IServiceProvider serviceProvider,
            ILogger<OrderExpiryScannerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderExpiryScannerService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScanAndExpireOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while scanning for expired orders");
                }

                await Task.Delay(_scanInterval, stoppingToken);
            }

            _logger.LogInformation("OrderExpiryScannerService stopped");
        }

        private async Task ScanAndExpireOrdersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            var now = DateTime.UtcNow;
            var expiryThreshold = now.Subtract(_expiryWindow);

            // Find orders that are:
            // 1. Payment method = Online
            // 2. Payment status = Pending or Failed
            // 3. Created more than 24 hours ago
            // 4. Not cancelled yet
            var expiredOrders = await orderRepository.FindAsync(o =>
                o.PaymentMethod == "Online" &&
                (o.PaymentStatus == "Pending" || o.PaymentStatus == "Failed") &&
                o.CreatedAt <= expiryThreshold &&
                o.Status != "Cancelled");

            if (!expiredOrders.Any())
            {
                _logger.LogInformation("No expired orders found");
                return;
            }

            _logger.LogInformation("Found {Count} expired orders to cancel", expiredOrders.Count());

            foreach (var order in expiredOrders)
            {
                try
                {
                    order.PaymentStatus = "Expired";
                    order.Status = "Cancelled";
                    order.CancelReason = "Payment expired after 24 hours";
                    order.CancelledAt = now;
                    order.UpdatedAt = now;

                    await orderRepository.UpdateAsync(order);
                    
                    _logger.LogInformation(
                        "Expired order {OrderId} cancelled. Created: {CreatedAt}, Payment Status: {PaymentStatus}",
                        order.Id, order.CreatedAt, order.PaymentStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel expired order {OrderId}", order.Id);
                }
            }

            _logger.LogInformation("Completed expiry scan. Cancelled {Count} orders", expiredOrders.Count());
        }
    }
}
