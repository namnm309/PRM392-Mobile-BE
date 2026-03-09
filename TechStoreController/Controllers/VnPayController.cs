using BAL.DTOs.Common;
using BAL.DTOs.VnPay;
using BAL.Services;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<VnPayController> _logger;

        public VnPayController(
            IVnPayService vnPayService,
            IOrderRepository orderRepository,
            ILogger<VnPayController> logger)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpPost("create-payment-url")]
        [ProducesResponseType(typeof(ApiResponse<VnPayResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VnPayResponseDto>>> CreatePaymentUrl([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<VnPayResponseDto>.ErrorResponse("User not authenticated"));

                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                    return NotFound(ApiResponse<VnPayResponseDto>.ErrorResponse("Order not found"));

                if (order.UserId != userId.Value)
                    return Forbid();

                if (order.PaymentMethod != "Online")
                    return BadRequest(ApiResponse<VnPayResponseDto>.ErrorResponse("Order payment method is not Online"));

                if (order.PaymentStatus == "Paid")
                    return BadRequest(ApiResponse<VnPayResponseDto>.ErrorResponse("Order has already been paid"));

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
                var paymentUrl = _vnPayService.CreatePaymentUrl(order, ipAddress);

                var response = new VnPayResponseDto { PaymentUrl = paymentUrl };
                return Ok(ApiResponse<VnPayResponseDto>.SuccessResponse(response, "Payment URL created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPAY payment URL for order {OrderId}", request.OrderId);
                return StatusCode(500, ApiResponse<VnPayResponseDto>.ErrorResponse("An error occurred while creating payment URL"));
            }
        }

        [HttpGet("ipn")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VnPayIpnResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> IpnCallback()
        {
            try
            {
                var queryParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
                var (isValid, vnpResponseCode, vnpTransactionNo, txnRef) = _vnPayService.ValidateCallback(queryParams);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid VNPAY checksum for TxnRef: {TxnRef}", txnRef);
                    return Ok(new VnPayIpnResponseDto { RspCode = "97", Message = "Invalid signature" });
                }

                var orders = await _orderRepository.FindAsync(o =>
                    o.PaymentMethod == "Online" &&
                    o.PaymentStatus == "Pending");

                var order = orders.FirstOrDefault(o =>
                    txnRef.StartsWith(o.Id.ToString("N").Substring(0, 8)));

                if (order == null)
                {
                    _logger.LogWarning("Order not found for TxnRef: {TxnRef}", txnRef);
                    return Ok(new VnPayIpnResponseDto { RspCode = "01", Message = "Order not found" });
                }

                var vnpAmount = queryParams.ContainsKey("vnp_Amount")
                    ? long.Parse(queryParams["vnp_Amount"]) / 100m
                    : 0m;

                if (vnpAmount != order.TotalAmount)
                {
                    _logger.LogWarning("Amount mismatch for TxnRef: {TxnRef}. Expected: {Expected}, Got: {Got}",
                        txnRef, order.TotalAmount, vnpAmount);
                    return Ok(new VnPayIpnResponseDto { RspCode = "04", Message = "Invalid amount" });
                }

                if (order.PaymentStatus != "Pending")
                {
                    return Ok(new VnPayIpnResponseDto { RspCode = "02", Message = "Order already confirmed" });
                }

                if (vnpResponseCode == "00")
                {
                    order.PaymentStatus = "Paid";
                    order.Status = "Processing";
                }
                else
                {
                    order.PaymentStatus = "Failed";
                }

                order.VnPayTransactionNo = vnpTransactionNo;
                order.PaymentDate = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation(
                    "VNPAY IPN processed. OrderId: {OrderId}, ResponseCode: {ResponseCode}, PaymentStatus: {PaymentStatus}",
                    order.Id, vnpResponseCode, order.PaymentStatus);

                return Ok(new VnPayIpnResponseDto { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPAY IPN callback");
                return Ok(new VnPayIpnResponseDto { RspCode = "99", Message = "Unknown error" });
            }
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public IActionResult PaymentReturn()
        {
            try
            {
                var queryParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
                var (isValid, vnpResponseCode, _, txnRef) = _vnPayService.ValidateCallback(queryParams);

                var isSuccess = isValid && vnpResponseCode == "00";
                var statusText = isSuccess ? "Thanh toán thành công" : "Thanh toán thất bại";
                var statusColor = isSuccess ? "#22c55e" : "#ef4444";

                var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>{statusText}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, sans-serif; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; background: #f9fafb; }}
        .container {{ text-align: center; padding: 2rem; }}
        .icon {{ font-size: 4rem; margin-bottom: 1rem; }}
        .status {{ font-size: 1.5rem; font-weight: 700; color: {statusColor}; margin-bottom: 0.5rem; }}
        .message {{ color: #6b7280; margin-bottom: 1.5rem; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>{(isSuccess ? "✅" : "❌")}</div>
        <div class='status'>{statusText}</div>
        <div class='message'>{(isSuccess ? "Đơn hàng của bạn đã được thanh toán thành công." : "Thanh toán không thành công. Vui lòng thử lại.")}</div>
    </div>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPAY return");
                return Content("<html><body><h1>Error</h1></body></html>", "text/html");
            }
        }
    }
}
