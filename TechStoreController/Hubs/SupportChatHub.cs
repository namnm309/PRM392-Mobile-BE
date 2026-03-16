using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TechStoreController.Hubs;

/// <summary>
/// SignalR Hub cho chat hỗ trợ real-time giữa user (mobile) và nhân viên (admin).
/// </summary>
[Authorize]
public class SupportChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, ChatConversation> _conversations = new();
    private static readonly ConcurrentDictionary<string, string> _userConnectionIds = new();
    private static readonly object _lock = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var userName = Context.User?.FindFirst("name")?.Value ?? Context.User?.Identity?.Name ?? "Khách";
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;

        if (isStaff)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
            await Clients.Group("Staff").SendAsync("StaffJoined");
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            _userConnectionIds[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, "Users");

            var conv = GetOrCreateConversation(userId, userName);
            conv.ConnectionId = Context.ConnectionId;
            conv.LastActivityAt = DateTime.UtcNow;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;

        if (!isStaff && !string.IsNullOrEmpty(userId))
        {
            _userConnectionIds.TryRemove(userId, out _);
            if (_conversations.TryGetValue(userId, out var conv))
            {
                conv.ConnectionId = null;
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// User gửi tin nhắn đến nhân viên.
    /// </summary>
    public async Task SendUserMessage(string content)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var userName = Context.User?.FindFirst("name")?.Value ?? Context.User?.Identity?.Name ?? "Khách";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(content))
        {
            await Clients.Caller.SendAsync("Error", "Invalid message");
            return;
        }

        var conv = GetOrCreateConversation(userId, userName);
        var msg = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conv.Id,
            SenderRole = "user",
            SenderId = userId,
            SenderName = userName,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        conv.Messages.Add(msg);
        conv.LastMessageAt = msg.CreatedAt;
        conv.LastMessagePreview = content.Trim().Length > 50 ? content.Trim()[..50] + "..." : content.Trim();
        conv.WaitingForStaff = true;
        conv.LastActivityAt = msg.CreatedAt;

        await Clients.Group("Staff").SendAsync("NewUserMessage", new
        {
            conversationId = conv.Id,
            userId,
            userName,
            message = new
            {
                id = msg.Id,
                content = msg.Content,
                createdAt = msg.CreatedAt,
                senderName = msg.SenderName
            },
            waitingCount = GetWaitingCount()
        });

        await Clients.Caller.SendAsync("MessageSent", new
        {
            id = msg.Id,
            content = msg.Content,
            createdAt = msg.CreatedAt
        });
    }

    /// <summary>
    /// Nhân viên gửi tin nhắn đến user.
    /// </summary>
    public async Task SendStaffMessage(string userId, string content)
    {
        var staffName = Context.User?.FindFirst("name")?.Value ?? Context.User?.Identity?.Name ?? "Nhân viên";
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;

        if (!isStaff || string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(content))
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized or invalid message");
            return;
        }

        if (!_conversations.TryGetValue(userId, out var conv))
        {
            await Clients.Caller.SendAsync("Error", "Conversation not found");
            return;
        }

        var msg = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conv.Id,
            SenderRole = "staff",
            SenderId = "staff",
            SenderName = staffName,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        conv.Messages.Add(msg);
        conv.LastMessageAt = msg.CreatedAt;
        conv.LastMessagePreview = content.Trim().Length > 50 ? content.Trim()[..50] + "..." : content.Trim();
        conv.WaitingForStaff = false;
        conv.LastActivityAt = msg.CreatedAt;

        await Clients.Group($"User_{userId}").SendAsync("ReceiveStaffMessage", new
        {
            id = msg.Id,
            content = msg.Content,
            createdAt = msg.CreatedAt,
            senderName = msg.SenderName
        });

        await Clients.Caller.SendAsync("MessageSent", new
        {
            id = msg.Id,
            content = msg.Content,
            createdAt = msg.CreatedAt
        });
    }

    /// <summary>
    /// Lấy danh sách cuộc hội thoại đang chờ (cho staff).
    /// </summary>
    public Task<List<ConversationSummary>> GetWaitingConversations()
    {
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;
        if (!isStaff)
            return Task.FromResult(new List<ConversationSummary>());

        var list = _conversations.Values
            .Where(c => c.Messages.Count > 0)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationSummary
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.UserName,
                LastMessagePreview = c.LastMessagePreview,
                LastMessageAt = c.LastMessageAt,
                MessageCount = c.Messages.Count,
                WaitingForStaff = c.WaitingForStaff
            })
            .ToList();

        return Task.FromResult(list);
    }

    /// <summary>
    /// User lấy tin nhắn của chính mình (gọi từ mobile).
    /// </summary>
    public Task<List<ChatMessageDto>> GetMyMessages()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;
        if (isStaff || string.IsNullOrEmpty(userId))
            return Task.FromResult(new List<ChatMessageDto>());
        return GetConversationMessages(userId);
    }

    /// <summary>
    /// Lấy lịch sử tin nhắn của một cuộc hội thoại.
    /// </summary>
    public Task<List<ChatMessageDto>> GetConversationMessages(string userId)
    {
        var callerUserId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;

        if (!isStaff && callerUserId != userId)
            return Task.FromResult(new List<ChatMessageDto>());

        if (!_conversations.TryGetValue(userId, out var conv))
            return Task.FromResult(new List<ChatMessageDto>());

        var dtos = conv.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderRole = m.SenderRole,
                SenderName = m.SenderName,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return Task.FromResult(dtos);
    }

    /// <summary>
    /// Đánh dấu đã đọc (staff đã mở chat).
    /// </summary>
    public async Task MarkConversationOpened(string userId)
    {
        var isStaff = Context.User?.IsInRole("Staff") == true || Context.User?.IsInRole("Admin") == true;
        if (!isStaff || !_conversations.TryGetValue(userId, out var conv))
            return;

        conv.WaitingForStaff = false;
        await Clients.Group("Staff").SendAsync("WaitingCountUpdated", GetWaitingCount());
    }

    private static ChatConversation GetOrCreateConversation(string userId, string userName)
    {
        return _conversations.GetOrAdd(userId, _ => new ChatConversation
        {
            Id = userId,
            UserId = userId,
            UserName = userName,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        });
    }

    private static int GetWaitingCount() => _conversations.Values.Count(c => c.WaitingForStaff && c.Messages.Count > 0);

    public static IReadOnlyList<ConversationSummary> GetAllConversationsForApi()
    {
        return _conversations.Values
            .Where(c => c.Messages.Count > 0)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationSummary
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.UserName,
                LastMessagePreview = c.LastMessagePreview,
                LastMessageAt = c.LastMessageAt,
                MessageCount = c.Messages.Count,
                WaitingForStaff = c.WaitingForStaff
            })
            .ToList();
    }

    public static ChatConversation GetOrCreateConversationApi(string userId, string userName)
        => GetOrCreateConversation(userId, userName);

    public static int GetWaitingCountApi() => GetWaitingCount();

    public static IReadOnlyList<ChatMessage> GetUserMessagesApi(string userId)
    {
        if (!_conversations.TryGetValue(userId, out var conv))
            return Array.Empty<ChatMessage>();
        return conv.Messages.OrderBy(m => m.CreatedAt).ToList();
    }
}

public class ChatConversation
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? ConnectionId { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public bool WaitingForStaff { get; set; }
}

public class ChatMessage
{
    public string Id { get; set; } = "";
    public string ConversationId { get; set; } = "";
    public string SenderRole { get; set; } = "";
    public string SenderId { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ChatMessageDto
{
    public string Id { get; set; } = "";
    public string Content { get; set; } = "";
    public string SenderRole { get; set; } = "";
    public string SenderName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ConversationSummary
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? LastMessagePreview { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessageCount { get; set; }
    public bool WaitingForStaff { get; set; }
}
