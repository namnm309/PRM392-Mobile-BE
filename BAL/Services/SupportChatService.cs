//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BAL.DTOs.SupportChat;
//using DAL.Data;
//using DAL.Models;
//using DAL.Repositories;
//using Microsoft.EntityFrameworkCore;

//namespace BAL.Services
//{
//    public class SupportChatService : ISupportChatService
//    {
//        private readonly TechStoreContext _db;
//        //private readonly MegaLlmGateway _llm;

//        public SupportChatService(TechStoreContext db, MegaLlmGateway llm)
//        {
//            _db = db;
//            _llm = llm;
//        }

//        public async Task<SupportChatSendResponse> SendAsync(SupportChatSendRequest req, CancellationToken ct = default)
//        {
//            var message = (req.Message ?? "").Trim();
//            if (string.IsNullOrWhiteSpace(message) || message.Length > 1000)
//                throw new ArgumentException("Message must be 1..1000 characters");

//            // Create or load session
//            SupportChatSession session;
//            if (req.SessionId is null)
//            {
//                session = new SupportChatSession();
//                _db.SupportChatSessions.Add(session);
//                await _db.SaveChangesAsync(ct);
//            }
//            else
//            {
//                session = await _db.SupportChatSessions
//                    .FirstOrDefaultAsync(x => x.SessionId == req.SessionId.Value, ct)
//                    ?? new SupportChatSession { SessionId = req.SessionId.Value };

//                if (_db.Entry(session).State == EntityState.Detached)
//                {
//                    _db.SupportChatSessions.Add(session);
//                    await _db.SaveChangesAsync(ct);
//                }
//            }

//            // Load last 10 for context
//            var history = await _db.SupportChatMessages
//                .Where(x => x.SessionId == session.SessionId)
//                .OrderByDescending(x => x.CreatedAt)
//                .Take(10)
//                .OrderBy(x => x.CreatedAt)
//                .ToListAsync(ct);

//            var messages = new List<object>
//        {
//            new { role = "system", content = "Bạn là chatbot CSKH TechStore. Trả lời tiếng Việt ngắn gọn, thân thiện. Nếu thiếu thông tin thì hỏi lại. Không bịa chính sách, không bịa trạng thái đơn hàng." }
//        };

//            foreach (var h in history)
//            {
//                messages.Add(new { role = "user", content = h.UserMessage });
//                messages.Add(new { role = "assistant", content = h.BotReply });
//            }

//            messages.Add(new { role = "user", content = message });

//            var reply = await _llm.AskAsync(messages, ct);

//            var row = new SupportChatMessage
//            {
//                SessionId = session.SessionId,
//                UserMessage = message,
//                BotReply = reply,
//                Model = "gpt-4o-mini",
//                CreatedAt = DateTime.UtcNow
//            };

//            session.UpdatedAt = DateTime.UtcNow;

//            _db.SupportChatMessages.Add(row);
//            await _db.SaveChangesAsync(ct);

//            return new SupportChatSendResponse(session.SessionId, row.Id, row.UserMessage, row.BotReply, row.CreatedAt);
//        }

//        public async Task<SupportChatHistoryResponse> GetHistoryAsync(Guid sessionId, int limit = 20, CancellationToken ct = default)
//        {
//            limit = Math.Clamp(limit, 1, 50);

//            var exists = await _db.SupportChatSessions.AnyAsync(x => x.SessionId == sessionId, ct);
//            if (!exists) throw new KeyNotFoundException("Session not found");

//            var items = await _db.SupportChatMessages
//                .Where(x => x.SessionId == sessionId)
//                .OrderByDescending(x => x.CreatedAt)
//                .Take(limit)
//                .OrderBy(x => x.CreatedAt)
//                .Select(x => new SupportChatHistoryItem(x.Id, x.UserMessage, x.BotReply, x.CreatedAt))
//                .ToListAsync(ct);

//            return new SupportChatHistoryResponse(sessionId, items);
//        }
//    }
//}
