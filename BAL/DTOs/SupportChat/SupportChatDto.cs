using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.DTOs.SupportChat
{
    public record SupportChatSendRequest(Guid? SessionId, string Message);

    public record SupportChatSendResponse(
        Guid SessionId,
        long MessageId,
        string UserMessage,
        string BotReply,
        DateTime CreatedAt
    );

    public record SupportChatHistoryItem(long Id, string UserMessage, string BotReply, DateTime CreatedAt);

    public record SupportChatHistoryResponse(Guid SessionId, List<SupportChatHistoryItem> Messages);
}
