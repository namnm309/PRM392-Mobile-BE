using BAL.DTOs;
using BAL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace TechStoreController.Controllers
{
    /// <summary>
    /// Controller để nhận và xử lý webhook từ Clerk
    /// </summary>
    [ApiController]
    [Route("api/webhook/clerk")]
    public class ClerkWebhookController : ControllerBase
    {
        private readonly ILogger<ClerkWebhookController> _logger;
        private readonly ClerkWebhookVerifier _webhookVerifier;
        private readonly IUserService _userService;

        public ClerkWebhookController(
            ILogger<ClerkWebhookController> logger,
            ClerkWebhookVerifier webhookVerifier,
            IUserService userService)
        {
            _logger = logger;
            _webhookVerifier = webhookVerifier;
            _userService = userService;
        }

        /// <summary>
        /// Endpoint để nhận webhook từ Clerk
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                // Đảm bảo buffering đã được enable
                if (!Request.Body.CanSeek)
                {
                    Request.EnableBuffering();
                }
                
                Request.Body.Position = 0;
                
                // Đọc raw body để verify signature
                string payload;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    payload = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(payload))
                {
                    _logger.LogWarning("Webhook payload is empty");
                    return BadRequest(new { error = "Payload is empty" });
                }

                _logger.LogInformation("Received webhook payload: {Length} bytes", payload.Length);

                // Lấy Svix headers từ request
                var svixId = Request.Headers["svix-id"].FirstOrDefault();
                var svixTimestamp = Request.Headers["svix-timestamp"].FirstOrDefault();
                var svixSignature = Request.Headers["svix-signature"].FirstOrDefault();

                _logger.LogInformation("Svix headers - Id: {Id}, Timestamp: {Timestamp}, Signature: {SignaturePreview}",
                    svixId ?? "null",
                    svixTimestamp ?? "null",
                    svixSignature?.Substring(0, Math.Min(20, svixSignature?.Length ?? 0)) ?? "null");

                // Verify signature sử dụng Svix SDK
                try
                {
                    _webhookVerifier.Verify(payload, svixId!, svixTimestamp!, svixSignature!);
                    _logger.LogInformation("Webhook signature verified successfully");
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Webhook verification failed - missing data: {Message}", ex.Message);
                    return BadRequest(new { error = "Missing required headers or payload" });
                }
                catch (Exception ex)
                {
                    // Catch tất cả exceptions từ Svix SDK verify
                    _logger.LogWarning("Webhook verification failed: {ExceptionType} - {Message}", 
                        ex.GetType().Name, ex.Message);
                    return Unauthorized(new { error = "Invalid signature", details = ex.Message });
                }

                // Deserialize payload
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };
                
                var webhookEvent = JsonSerializer.Deserialize<ClerkWebhookDto>(payload, jsonOptions);

                if (webhookEvent?.Data == null)
                {
                    _logger.LogWarning("Webhook event or data is null");
                    return BadRequest(new { error = "Invalid webhook payload" });
                }

                _logger.LogInformation("Received webhook event: {EventType}, UserId: {UserId}", 
                    webhookEvent.Type, webhookEvent.Data.Id);

                // Xử lý các event types
                try
                {
                    switch (webhookEvent.Type)
                    {
                        case "user.created":
                            await HandleUserCreated(webhookEvent.Data);
                            break;

                        case "user.updated":
                            await HandleUserUpdated(webhookEvent.Data);
                            break;

                        case "user.deleted":
                            await HandleUserDeleted(webhookEvent.Data);
                            break;

                        default:
                            _logger.LogInformation("Unhandled webhook event type: {EventType}", webhookEvent.Type);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing webhook event {EventType} for user {UserId}: {Message}", 
                        webhookEvent.Type, webhookEvent.Data.Id, ex.Message);
                    return StatusCode(500, new { 
                        error = "Failed to process webhook event", 
                        eventType = webhookEvent.Type,
                        userId = webhookEvent.Data.Id,
                        details = ex.Message 
                    });
                }

                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize webhook payload");
                return BadRequest(new { error = "Invalid JSON payload", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing webhook: {Message}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        private async Task HandleUserCreated(ClerkWebhookDataDto data)
        {
            _logger.LogInformation("Creating user: ClerkId={ClerkId}, Email={Email}", 
                data.Id, data.EmailAddresses?.FirstOrDefault()?.EmailAddress ?? "N/A");
            
            var user = await _userService.CreateUserAsync(data);
            
            if (user != null)
            {
                _logger.LogInformation("User created successfully: ClerkId={ClerkId}, DbId={DbId}, Email={Email}", 
                    data.Id, user.Id, user.Email);
            }
            else
            {
                _logger.LogWarning("UserService returned null when creating user: ClerkId={ClerkId}", data.Id);
            }
        }

        private async Task HandleUserUpdated(ClerkWebhookDataDto data)
        {
            _logger.LogInformation("Updating user: ClerkId={ClerkId}", data.Id);
            
            var user = await _userService.UpdateUserAsync(data);
            
            if (user != null)
            {
                _logger.LogInformation("User updated successfully: ClerkId={ClerkId}, DbId={DbId}", 
                    data.Id, user.Id);
            }
            else
            {
                _logger.LogWarning("UserService returned null when updating user: ClerkId={ClerkId}", data.Id);
            }
        }

        private async Task HandleUserDeleted(ClerkWebhookDataDto data)
        {
            if (string.IsNullOrEmpty(data.Id))
            {
                _logger.LogWarning("Cannot delete user: ClerkId is empty");
                return;
            }

            _logger.LogInformation("Deleting user: ClerkId={ClerkId}", data.Id);
            
            var deleted = await _userService.DeleteUserAsync(data.Id);
            _logger.LogInformation("User deleted: ClerkId={ClerkId}, Success={Success}", 
                data.Id, deleted);
        }
    }
}
