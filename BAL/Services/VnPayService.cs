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
            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", _version },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _tmnCode },
                { "vnp_Amount", ((long)(order.TotalAmount * 100)).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.Id.ToString("N").Substring(0, 8) + DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "vnp_OrderInfo", $"Thanh toan don hang {order.Id.ToString().Substring(0, 8).ToUpper()}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _returnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            var hashData = new StringBuilder();
            var query = new StringBuilder();
            var first = true;

            foreach (var kv in vnpParams)
            {
                if (!first)
                {
                    hashData.Append('&');
                    query.Append('&');
                }

                hashData.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value));
                query.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value));
                first = false;
            }

            var secureHash = HmacSha512(_hashSecret, hashData.ToString());
            query.Append("&vnp_SecureHash=" + secureHash);

            return _paymentUrl + "?" + query;
        }

        public (bool IsValid, string VnpResponseCode, string VnpTransactionNo, string TxnRef) ValidateCallback(IDictionary<string, string> queryParams)
        {
            var vnpSecureHash = queryParams.ContainsKey("vnp_SecureHash")
                ? queryParams["vnp_SecureHash"]
                : string.Empty;

            var vnpResponseCode = queryParams.ContainsKey("vnp_ResponseCode")
                ? queryParams["vnp_ResponseCode"]
                : string.Empty;

            var vnpTransactionNo = queryParams.ContainsKey("vnp_TransactionNo")
                ? queryParams["vnp_TransactionNo"]
                : string.Empty;

            var txnRef = queryParams.ContainsKey("vnp_TxnRef")
                ? queryParams["vnp_TxnRef"]
                : string.Empty;

            var filteredParams = new SortedDictionary<string, string>();
            foreach (var kv in queryParams)
            {
                if (!kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase) &&
                    kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                {
                    filteredParams[kv.Key] = kv.Value;
                }
            }

            var hashData = new StringBuilder();
            var first = true;
            foreach (var kv in filteredParams)
            {
                if (!first) hashData.Append('&');
                hashData.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value));
                first = false;
            }

            var computedHash = HmacSha512(_hashSecret, hashData.ToString());
            var isValid = computedHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);

            return (isValid, vnpResponseCode, vnpTransactionNo, txnRef);
        }

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
