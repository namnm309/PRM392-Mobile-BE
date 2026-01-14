using BAL.DTOs;
using BAL.DTOs.SupportChat;
using BAL.Services;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers;

[ApiController]
[Route("api/v1/support-chat")]
public class SupportChatController : ControllerBase
{
    private readonly ISupportChatService _svc;

    public SupportChatController(ISupportChatService svc)
    {
        _svc = svc;
    }

    [HttpPost("messages")]
    public async Task<ActionResult<SupportChatSendResponse>> Send([FromBody] SupportChatSendRequest req, CancellationToken ct)
    {
        var res = await _svc.SendAsync(req, ct);
        return Ok(res);
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<SupportChatHistoryResponse>> History(Guid sessionId, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var res = await _svc.GetHistoryAsync(sessionId, limit, ct);
        return Ok(res);
    }
}
