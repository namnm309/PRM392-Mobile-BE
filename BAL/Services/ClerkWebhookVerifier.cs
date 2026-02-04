using Svix;
using System.Net;

namespace BAL.Services
{
    /// <summary>
    /// Service để verify webhook signature từ Clerk (sử dụng Svix SDK)
    /// </summary>
    public class ClerkWebhookVerifier
    {
        private readonly Webhook _webhook;
        private readonly string _webhookSecret;

        public ClerkWebhookVerifier(string webhookSecret)
        {
            _webhookSecret = webhookSecret ?? throw new ArgumentNullException(nameof(webhookSecret));
            _webhook = new Webhook(webhookSecret);
        }

        /// <summary>
        /// Verify webhook signature từ Clerk sử dụng Svix SDK
        /// </summary>
        /// <param name="payload">Raw payload body từ request</param>
        /// <param name="svixId">Header svix-id</param>
        /// <param name="svixTimestamp">Header svix-timestamp</param>
        /// <param name="svixSignature">Header svix-signature</param>
        /// <exception cref="Exception">Thrown khi signature không hợp lệ</exception>
        public void Verify(string payload, string svixId, string svixTimestamp, string svixSignature)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentException("Payload is empty", nameof(payload));
            }

            if (string.IsNullOrEmpty(svixId) || 
                string.IsNullOrEmpty(svixTimestamp) || 
                string.IsNullOrEmpty(svixSignature))
            {
                throw new ArgumentException("Missing required Svix headers");
            }

            var headers = new WebHeaderCollection
            {
                ["svix-id"] = svixId,
                ["svix-timestamp"] = svixTimestamp,
                ["svix-signature"] = svixSignature
            };

            // Svix SDK sẽ throw exception nếu signature không hợp lệ
            // SDK cũng tự động verify timestamp để chống replay attacks
            _webhook.Verify(payload, headers);
        }

        /// <summary>
        /// Lấy preview của secret để logging (không expose full secret)
        /// </summary>
        public string GetSecretPreview()
        {
            if (string.IsNullOrEmpty(_webhookSecret))
            {
                return "Not configured";
            }
            return _webhookSecret.Length > 10 
                ? $"{_webhookSecret.Substring(0, 10)}..." 
                : "***";
        }
    }
}
