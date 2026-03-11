using System.Net;
using System.Security.Cryptography;
using System.Text;
using DAL.Models;
using Microsoft.Extensions.Configuration;

namespace BAL.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _paymentUrl;
        private readonly string _returnUrl;
        private readonly string _version;

        private static readonly TimeZoneInfo VietnamTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public VnPayService(IConfiguration configuration)
        {
            _tmnCode = configuration["VnPay:TmnCode"]
                ?? throw new InvalidOperationException("VnPay:TmnCode is not configured");
            _hashSecret = configuration["VnPay:HashSecret"]
                ?? throw new InvalidOperationException("VnPay:HashSecret is not configured");
            _paymentUrl = configuration["VnPay:PaymentUrl"]
                ?? throw new InvalidOperationException("VnPay:PaymentUrl is not configured");
            _returnUrl = configuration["VnPay:ReturnUrl"]
                ?? throw new InvalidOperationException("VnPay:ReturnUrl is not configured");
            _version = configuration["VnPay:Version"] ?? "2.1.0";
        }

        public string CreatePaymentUrl(Order order, string ipAddress)
        {
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", _version },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _tmnCode },
                { "vnp_Amount", ((long)(order.TotalAmount * 100)).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.Id.ToString("N")[..8] + DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "vnp_OrderInfo", $"Thanh toan don hang {order.Id.ToString()[..8].ToUpper()}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _returnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", vietnamNow.ToString("yyyyMMddHHmmss") },
                { "vnp_ExpireDate", vietnamNow.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            // Hash data và query string dùng chung một chuỗi đã URL-encode, sort theo key
            var hashData = BuildQueryString(vnpParams);
            var secureHash = HmacSha512(_hashSecret, hashData);

            var queryString = $"{hashData}&vnp_SecureHash={secureHash}";
            return $"{_paymentUrl}?{queryString}";
        }

        public (bool IsValid, string VnpResponseCode, string VnpTransactionNo, string TxnRef) ValidateCallback(
            IDictionary<string, string> queryParams)
        {
            queryParams.TryGetValue("vnp_SecureHash", out var vnpSecureHash);
            queryParams.TryGetValue("vnp_ResponseCode", out var vnpResponseCode);
            queryParams.TryGetValue("vnp_TransactionNo", out var vnpTransactionNo);
            queryParams.TryGetValue("vnp_TxnRef", out var txnRef);
            vnpSecureHash ??= "";
            vnpResponseCode ??= "";
            vnpTransactionNo ??= "";
            txnRef ??= "";

            var filteredParams = new SortedDictionary<string, string>();
            foreach (var kv in queryParams)
            {
                if (kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                    !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    filteredParams[kv.Key] = kv.Value;
                }
            }

            var hashData = BuildQueryString(filteredParams);
            var computedHash = HmacSha512(_hashSecret, hashData);
            var isValid = computedHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);

            return (isValid, vnpResponseCode, vnpTransactionNo, txnRef);
        }

        private static string BuildQueryString(SortedDictionary<string, string> data)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var kv in data)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                if (!first) sb.Append('&');

                var encodedKey = WebUtility.UrlEncode(kv.Key);
                var encodedValue = WebUtility.UrlEncode(kv.Value);

                sb.Append(encodedKey)
                  .Append('=')
                  .Append(encodedValue);

                first = false;
            }
            return sb.ToString();
        }

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
