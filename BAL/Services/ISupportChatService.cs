using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BAL.DTOs.SupportChat;

namespace BAL.Services
{
    public interface ISupportChatService
    {
        Task<SupportChatSendResponse> SendAsync(SupportChatSendRequest req, CancellationToken ct = default);
        Task<SupportChatHistoryResponse> GetHistoryAsync(Guid sessionId, int limit = 20, CancellationToken ct = default);
    }
}
