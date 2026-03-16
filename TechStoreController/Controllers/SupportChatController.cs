using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechStoreController.Hubs;

namespace TechStoreController.Controllers;

/// <summary>
/// REST API cho chat hỗ trợ — dùng cho mobile thay vì SignalR.
/// Tái sử dụng cùng in-memory store của SupportChatHub.
/// Staff trên Dashboard vẫn nhận tin qua SignalR real-time.
/// </summary>
[ApiController]
[Route("api/support-chat")]
[Authorize]
[Produces("application/json")]
public class SupportChatController : ControllerBase
{
    private readonly IHubContext<SupportChatHub> _hubContext;

    public SupportChatController(IHubContext<SupportChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// User gửi tin nhắn (thay cho SignalR SendUserMessage)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Khách";

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Không xác định được user" });

        if (string.IsNullOrWhiteSpace(request?.Content))
            return BadRequest(new { message = "Nội dung tin nhắn không được rỗng" });

        var conv = SupportChatHub.GetOrCreateConversationApi(userId, userName);
        var msg = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conv.Id,
            SenderRole = "user",
            SenderId = userId,
            SenderName = userName,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        conv.Messages.Add(msg);
        conv.LastMessageAt = msg.CreatedAt;
        conv.LastMessagePreview = msg.Content.Length > 50 ? msg.Content[..50] + "..." : msg.Content;
        conv.WaitingForStaff = true;
        conv.LastActivityAt = msg.CreatedAt;

        await _hubContext.Clients.Group("Staff").SendAsync("NewUserMessage", new
        {
            conversationId = conv.Id,
            userId,
            userName,
            message = new { id = msg.Id, content = msg.Content, createdAt = msg.CreatedAt, senderName = msg.SenderName },
            waitingCount = SupportChatHub.GetWaitingCountApi()
        });

        return Ok(new
        {
            id = msg.Id,
            content = msg.Content,
            senderRole = msg.SenderRole,
            senderName = msg.SenderName,
            createdAt = msg.CreatedAt
        });
    }

    /// <summary>
    /// User lấy tin nhắn của mình (polling endpoint)
    /// </summary>
    [HttpGet("messages")]
    public IActionResult GetMessages([FromQuery] string? after)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Không xác định được user" });

        var messages = SupportChatHub.GetUserMessagesApi(userId);

        if (!string.IsNullOrEmpty(after))
        {
            var afterDate = DateTime.TryParse(after, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            messages = messages.Where(m => m.CreatedAt > afterDate).ToList();
        }

        return Ok(messages.Select(m => new
        {
            id = m.Id,
            content = m.Content,
            senderRole = m.SenderRole,
            senderName = m.SenderName,
            createdAt = m.CreatedAt
        }));
    }
}

public class SendMessageRequest
{
    public string Content { get; set; } = "";
}
