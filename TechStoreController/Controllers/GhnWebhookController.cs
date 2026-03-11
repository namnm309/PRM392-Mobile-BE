/*
 * GHN WEBHOOK CONTROLLER - COMMENTED OUT
 * 
 * Lý do: GHN không hỗ trợ webhook config, nên hiện tại sử dụng pull model (user/admin chủ động gọi API).
 * Controller này được giữ lại để tham khảo trong tương lai nếu GHN hỗ trợ webhook.
 * 
 * NOTE: Logic mapping GHN status đã được extract sang BAL.Helpers.GhnStatusMapper
 * để tái sử dụng trong pull model (OrdersController.GetGhnOrderStatus)
 */

using BAL.DTOs.Common;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechStoreController.Controllers
{
    /*
    /// <summary>
    /// Controller để nhận và xử lý webhook từ GHN
    /// COMMENTED OUT - GHN không hỗ trợ webhook config, sử dụng pull model thay thế
    /// </summary>
    [ApiController]
    [Route("api/webhook/ghn")]
    [Produces("application/json")]
    public class GhnWebhookController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GhnWebhookController> _logger;

        public GhnWebhookController(
            IOrderRepository orderRepository,
            ILogger<GhnWebhookController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint để nhận webhook callback trạng thái đơn hàng từ GHN
        /// </summary>
        [HttpPost("order-status")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GhnWebhookResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleOrderStatusCallback()
        {
            try
            {
                // Read raw body
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                _logger.LogInformation("Received GHN webhook payload: {Length} bytes", payload.Length);

                if (string.IsNullOrEmpty(payload))
                {
                    _logger.LogWarning("GHN webhook payload is empty");
                    return Ok(new GhnWebhookResponse { Code = "99", Message = "Empty payload" });
                }

                // Parse JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var ghnEvent = JsonSerializer.Deserialize<GhnOrderStatusCallbackDto>(payload, options);

                if (ghnEvent == null)
                {
                    _logger.LogWarning("Failed to parse GHN webhook payload");
                    return Ok(new GhnWebhookResponse { Code = "99", Message = "Invalid payload format" });
                }

                _logger.LogInformation(
                    "GHN webhook event: Type={Type}, Status={Status}, OrderCode={OrderCode}, ClientOrderCode={ClientOrderCode}",
                    ghnEvent.Type, ghnEvent.Status, ghnEvent.OrderCode, ghnEvent.ClientOrderCode);

                // Find order by GhnOrderCode or ClientOrderCode
                var orders = await _orderRepository.FindAsync(o =>
                    o.GhnOrderCode == ghnEvent.OrderCode ||
                    (!string.IsNullOrEmpty(ghnEvent.ClientOrderCode) && o.Id.ToString() == ghnEvent.ClientOrderCode));

                var order = orders.FirstOrDefault();

                if (order == null)
                {
                    _logger.LogWarning(
                        "Order not found for GHN OrderCode: {OrderCode}, ClientOrderCode: {ClientOrderCode}",
                        ghnEvent.OrderCode, ghnEvent.ClientOrderCode);
                    return Ok(new GhnWebhookResponse { Code = "01", Message = "Order not found" });
                }

                // Business rule: Only allow shipping status updates for confirmed orders
                if (order.Status != "Confirmed" && order.Status != "Shipping" && order.Status != "Delivered")
                {
                    _logger.LogWarning(
                        "Order {OrderId} status is {Status}, not eligible for GHN callback updates",
                        order.Id, order.Status);
                    return Ok(new GhnWebhookResponse { Code = "02", Message = "Order not confirmed yet" });
                }

                // Check idempotency: If same status already applied, skip update
                var currentMappedStatus = MapGhnStatusToOrderStatus(ghnEvent.Status);
                if (order.Status == currentMappedStatus)
                {
                    _logger.LogInformation(
                        "Order {OrderId} already has status {Status}, skipping duplicate update",
                        order.Id, currentMappedStatus);
                    return Ok(new GhnWebhookResponse { Code = "00", Message = "Already processed" });
                }

                // Map GHN status to internal order status
                var previousStatus = order.Status;
                order.Status = currentMappedStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Update delivery timestamp if status is delivered
                if (ghnEvent.Status == "delivered" && order.DeliveredAt == null)
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }

                // Append notes about status change
                var statusChangeNote = $"[GHN {DateTime.UtcNow:yyyy-MM-dd HH:mm}]: {previousStatus} -> {currentMappedStatus}";
                if (!string.IsNullOrEmpty(ghnEvent.Reason))
                {
                    statusChangeNote += $" | Reason: {ghnEvent.Reason}";
                }
                order.Notes = string.IsNullOrEmpty(order.Notes)
                    ? statusChangeNote
                    : $"{order.Notes}\n{statusChangeNote}";

                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation(
                    "Updated order {OrderId} from {PreviousStatus} to {NewStatus} via GHN callback",
                    order.Id, previousStatus, currentMappedStatus);

                return Ok(new GhnWebhookResponse { Code = "00", Message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GHN webhook callback");
                return Ok(new GhnWebhookResponse { Code = "99", Message = "Internal error" });
            }
        }

        /// <summary>
        /// Map GHN shipping status to internal order status
        /// </summary>
        private static string MapGhnStatusToOrderStatus(string ghnStatus)
        {
            return ghnStatus?.ToLower() switch
            {
                "ready_to_pick" => "Confirmed",
                "picking" => "Shipping",
                "picked" => "Shipping",
                "storing" => "Shipping",
                "transporting" => "Shipping",
                "sorting" => "Shipping",
                "delivering" => "Shipping",
                "delivered" => "Delivered",
                "delivery_fail" => "Shipping",
                "waiting_to_return" => "Shipping",
                "return" => "Cancelled",
                "returned" => "Cancelled",
                "cancel" => "Cancelled",
                "exception" => "Shipping",
                "lost" => "Cancelled",
                "damage" => "Shipping",
                _ => "Shipping" // Default to Shipping for unknown statuses
            };
        }
    }

    #region GHN Webhook DTOs

    /// <summary>
    /// DTO for GHN order status callback payload
    /// Theo docs: https://api.ghn.vn/home/docs/detail?id=84
    /// </summary>
    public class GhnOrderStatusCallbackDto
    {
        [JsonPropertyName("CODAmount")]
        public decimal CODAmount { get; set; }

        [JsonPropertyName("CODTransferDate")]
        public DateTime? CODTransferDate { get; set; }

        [JsonPropertyName("ClientOrderCode")]
        public string ClientOrderCode { get; set; } = "";

        [JsonPropertyName("ConvertedWeight")]
        public int ConvertedWeight { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("OrderCode")]
        public string OrderCode { get; set; } = "";

        [JsonPropertyName("Reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("ReasonCode")]
        public string? ReasonCode { get; set; }

        [JsonPropertyName("ShopID")]
        public int ShopID { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("Time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("Warehouse")]
        public string? Warehouse { get; set; }
    }

    public class GhnWebhookResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "00";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "Success";
    }

    #endregion
    */
}
