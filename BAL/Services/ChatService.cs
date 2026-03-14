using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BAL.DTOs.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ChatService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra cấu hình Mega LLM (OpenAI-compatible)
        /// </summary>
        public async Task<(bool ApiKeyConfigured, string Provider, string BaseUrl, string ModelId, string? TestError)> GetDiagnosticAsync(CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["MegaLLM:ApiKey"];
            var baseUrl = _configuration["MegaLLM:BaseUrl"]?.TrimEnd('/') ?? "https://ai.megallm.io/v1";
            var modelId = _configuration["MegaLLM:ModelId"] ?? "openai-gpt-oss-20b";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "MegaLLM", baseUrl, modelId, "MegaLLM:ApiKey chưa được cấu hình. Thêm MegaLLM__ApiKey vào env hoặc appsettings.");
            }

            try
            {
                var url = $"{baseUrl}/chat/completions";
                var body = new MegaLLMRequest
                {
                    Model = modelId,
                    Messages = new List<object> { new MegaLLMTextMessage { Role = "user", Content = "Hello" } },
                    MaxTokens = 5,
                    Temperature = 0.1
                };
                var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(jsonBody, Encoding.UTF8, "application/json") };
                req.Headers.Add("Authorization", $"Bearer {apiKey}");
                var response = await _httpClient.SendAsync(req, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(cancellationToken);
                    return (true, "MegaLLM", baseUrl, modelId, $"Mega LLM trả {(int)response.StatusCode}: {err}");
                }
                return (true, "MegaLLM", baseUrl, modelId, null);
            }
            catch (Exception ex)
            {
                return (true, "MegaLLM", baseUrl, modelId, ex.Message);
            }
        }

        /// <summary>
        /// Gửi tin nhắn đến Mega LLM (OpenAI-compatible) và nhận phản hồi.
        /// </summary>
        public async Task<ChatResponseDto> SendChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["MegaLLM:ApiKey"];
            var baseUrl = _configuration["MegaLLM:BaseUrl"]?.TrimEnd('/') ?? "https://ai.megallm.io/v1";
            var maxTokensStr = _configuration["MegaLLM:MaxTokens"];
            var maxTokens = int.TryParse(maxTokensStr, out var mt) ? mt : 5000;
            var tempStr = _configuration["MegaLLM:Temperature"];
            var temperature = double.TryParse(tempStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var t) ? t : 1.5;

            var hasImage = !string.IsNullOrWhiteSpace(request.ImageBase64);
            var modelId = _configuration["MegaLLM:ModelId"] ?? "openai-gpt-oss-20b";
            var requestModel = request.Model?.Trim();
            if (!string.IsNullOrWhiteSpace(requestModel))
                modelId = requestModel;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("MegaLLM:ApiKey chưa được cấu hình. Thêm MegaLLM__ApiKey vào env hoặc appsettings.");
            }

            var systemPrompt = request.SystemPrompt?.Trim();
            if (string.IsNullOrEmpty(systemPrompt))
            {
                systemPrompt = "Bạn là trợ lý AI của TechStore - cửa hàng công nghệ. Bạn giúp người dùng tìm sản phẩm, so sánh giá, tư vấn mua hàng. Trả lời ngắn gọn bằng tiếng Việt.";
            }

            var messages = new List<object>();
            messages.Add(new MegaLLMTextMessage { Role = "system", Content = systemPrompt });

            if (request.Messages != null && request.Messages.Count > 0)
            {
                var historyMessages = request.Messages
                    .Where(m => !string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                for (int i = 0; i < historyMessages.Count; i++)
                {
                    var msg = historyMessages[i];
                    bool isLastUserMsg = hasImage
                        && i == historyMessages.Count - 1
                        && string.Equals(msg.Role, "user", StringComparison.OrdinalIgnoreCase);

                    if (isLastUserMsg)
                    {
                        var visionContent = new List<object>
                        {
                            new Dictionary<string, object> { ["type"] = "text", ["text"] = msg.Content ?? "" },
                            new Dictionary<string, object>
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new Dictionary<string, object>
                                {
                                    ["url"] = BuildImageDataUrl(request.ImageBase64!, request.ImageFormat),
                                    ["detail"] = "high"
                                }
                            }
                        };
                        messages.Add(new MegaLLMVisionMessage { Role = "user", Content = visionContent });
                    }
                    else
                    {
                        messages.Add(new MegaLLMTextMessage { Role = msg.Role ?? "user", Content = msg.Content ?? "" });
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.Message))
            {
                if (hasImage)
                {
                    var visionContent = new List<object>
                    {
                        new Dictionary<string, object> { ["type"] = "text", ["text"] = request.Message },
                        new Dictionary<string, object>
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new Dictionary<string, object>
                            {
                                ["url"] = BuildImageDataUrl(request.ImageBase64!, request.ImageFormat),
                                ["detail"] = "high"
                            }
                        }
                    };
                    messages.Add(new MegaLLMVisionMessage { Role = "user", Content = visionContent });
                }
                else
                {
                    messages.Add(new MegaLLMTextMessage { Role = "user", Content = request.Message });
                }
            }
            else
            {
                throw new ArgumentException("Message hoặc Messages không được để trống");
            }

            var megaRequest = new MegaLLMRequest
            {
                Model = modelId,
                Messages = messages,
                MaxTokens = maxTokens,
                Temperature = temperature
            };

            var url = $"{baseUrl}/chat/completions";
            var jsonBody = JsonSerializer.Serialize(megaRequest, JsonOptions);
            _logger.LogInformation("Mega LLM request Model: {Model}, HasImage: {HasImage}, MessageCount: {Count}", modelId, hasImage, messages.Count);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
            requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Mega LLM API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                throw new HttpRequestException($"Mega LLM trả lỗi {(int)response.StatusCode}: {errorBody}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<MegaLLMResponse>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Không thể đọc phản hồi từ Mega LLM");

            var text = apiResponse.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
            var usage = apiResponse.Usage != null
                ? new ChatUsageDto
                {
                    PromptTokens = apiResponse.Usage.PromptTokens,
                    CompletionTokens = apiResponse.Usage.CompletionTokens,
                    TotalTokens = apiResponse.Usage.TotalTokens
                }
                : null;

            return new ChatResponseDto { Content = text, Usage = usage };
        }

        private static string BuildImageDataUrl(string base64, string? format)
        {
            var mime = string.Equals(format, "png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            return $"data:{mime};base64,{base64}";
        }

        #region Mega LLM Models

        private class MegaLLMRequest
        {
            public string Model { get; set; } = "";
            public List<object> Messages { get; set; } = new();
            public int MaxTokens { get; set; }
            public double Temperature { get; set; }
        }

        private class MegaLLMTextMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }

        private class MegaLLMVisionMessage
        {
            public string Role { get; set; } = "";
            public List<object> Content { get; set; } = new();
        }

        private class MegaLLMResponse
        {
            public List<MegaLLMChoice>? Choices { get; set; }
            public MegaLLMUsage? Usage { get; set; }
        }

        private class MegaLLMChoice
        {
            public MegaLLMTextMessage? Message { get; set; }
        }

        private class MegaLLMUsage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        #endregion
    }
}
