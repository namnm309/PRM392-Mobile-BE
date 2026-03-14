using BAL.DTOs.Chat;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers;

/// <summary>
/// AI Chat Bot - tích hợp Mega LLM (OpenAI-compatible)
/// </summary>
[ApiController]
[Route("api/chat")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra endpoint SignalR Hub (chat với nhân viên)
    /// </summary>
    [HttpGet("signalr-status")]
    [AllowAnonymous]
    public IActionResult SignalRStatus()
    {
        var baseUrl = Request.Scheme + "://" + Request.Host;
        var hubUrl = baseUrl.TrimEnd('/') + "/hubs/support-chat";
        return Ok(new { hubUrl, message = "Bật Web Sockets và ARR Affinity trên Azure nếu gặp 404/Timeout." });
    }

    /// <summary>
    /// Kiểm tra cấu hình Mega LLM
    /// </summary>
    [HttpGet("diagnostic")]
    [AllowAnonymous]
    public async Task<IActionResult> Diagnostic(CancellationToken cancellationToken = default)
    {
        var (apiKeyConfigured, provider, baseUrl, modelId, testError) = await _chatService.GetDiagnosticAsync(cancellationToken);
        return Ok(new
        {
            apiKeyConfigured,
            provider,
            baseUrl,
            modelId,
            status = testError == null ? "OK" : "Error",
            error = testError,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gửi tin nhắn đến AI chat bot và nhận phản hồi
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request không được để trống" });
            }

            var response = await _chatService.SendChatAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ApiKey") || ex.Message.Contains("chưa được cấu hình"))
        {
            _logger.LogError(ex, "Mega LLM ApiKey chưa cấu hình");
            return StatusCode(503, new { message = "AI chưa được cấu hình. Liên hệ quản trị viên." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Mega LLM API error");
            return StatusCode(502, new { message = "Không thể kết nối AI. Vui lòng thử lại sau.", detail = ex.Message });
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Mega LLM request cancelled/timeout");
            return StatusCode(504, new { message = "Yêu cầu quá thời gian. Vui lòng thử lại." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat");
            return StatusCode(500, new { message = "Lỗi nội bộ. Vui lòng thử lại sau." });
        }
    }
}
