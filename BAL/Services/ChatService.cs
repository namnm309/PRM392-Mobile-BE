using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BAL.DTOs.Chat;
using BAL.DTOs.Product;
using BAL.DTOs.Category;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        private static readonly string[] VietnameseStopWords = { "có", "là", "gì", "nào", "bao", "nhiêu", "cho", "tôi", "mình", "bạn", "của", "và", "hoặc", "với", "từ", "đến", "các", "một", "những", "này", "đó", "không", "được", "hay", "muốn", "cần", "tìm", "mua", "xem", "giá", "thế", "ra", "sao", "ạ", "ơi", "nhé", "ạ" };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ChatService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger,
            IProductService productService, ICategoryService categoryService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
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

            var lastUserMessage = GetLastUserMessage(request);
            var dbContext = await GetProductAndCategoryContextAsync(lastUserMessage, cancellationToken);
            if (!string.IsNullOrEmpty(dbContext))
            {
                systemPrompt += "\n\n" + dbContext;
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

        private static string? GetLastUserMessage(ChatRequestDto request)
        {
            if (request.Messages != null && request.Messages.Count > 0)
            {
                var last = request.Messages
                    .Where(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))
                    .LastOrDefault();
                return last?.Content?.Trim();
            }
            return request.Message?.Trim();
        }

        private async Task<string> GetProductAndCategoryContextAsync(string? userMessage, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            try
            {
                var searchTerms = ExtractSearchTerms(userMessage);
                var products = new HashSet<ProductResponseDto>(new ProductIdComparer());
                const int maxProducts = 25;

                if (searchTerms.Count > 0)
                {
                    foreach (var term in searchTerms.Take(5))
                    {
                        if (products.Count >= maxProducts) break;
                        var found = await _productService.SearchProductsAsync(name: term, isActive: true);
                        foreach (var p in found.Take(maxProducts - products.Count))
                            products.Add(p);
                    }
                }

                if (products.Count == 0 && !string.IsNullOrWhiteSpace(userMessage))
                {
                    var found = await _productService.SearchProductsAsync(name: userMessage.Trim(), isActive: true);
                    foreach (var p in found.Take(maxProducts))
                        products.Add(p);
                }

                if (products.Count == 0)
                {
                    var all = await _productService.GetAllProductsAsync(isActive: true);
                    foreach (var p in all.Take(maxProducts))
                        products.Add(p);
                }

                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoryNames = categories.Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();

                sb.AppendLine("=== DỮ LIỆU SẢN PHẨM VÀ DANH MỤC HIỆN CÓ (lấy từ cơ sở dữ liệu thực) ===");
                sb.AppendLine("DANH MỤC: " + string.Join(", ", categoryNames));

                if (products.Count > 0)
                {
                    sb.AppendLine("\nSẢN PHẨM (ID, Tên, Giá gốc, Giá khuyến mãi nếu có, Tồn kho, Danh mục, Thương hiệu):");
                    foreach (var p in products.Take(maxProducts))
                    {
                        var priceStr = p.DiscountPrice.HasValue && p.DiscountPrice < p.Price
                            ? $"{p.Price:N0}đ → {p.DiscountPrice:N0}đ"
                            : $"{p.Price:N0}đ";
                        sb.AppendLine($"- ID:{p.Id} | {p.Name} | {priceStr} | Còn:{p.Stock} | {(p.Category?.Name ?? "-")} | {(p.Brand?.Name ?? "-")}");
                    }
                }

                sb.AppendLine("\nHãy dùng chính xác dữ liệu trên khi tư vấn: tên sản phẩm, giá, tồn kho. Nếu không có sản phẩm phù hợp, nói rõ và gợi ý danh mục có sẵn.");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không lấy được dữ liệu DB cho chatbot");
                return string.Empty;
            }
        }

        private static List<string> ExtractSearchTerms(string? message)
        {
            if (string.IsNullOrWhiteSpace(message)) return new List<string>();

            var cleaned = Regex.Replace(message, @"[^\p{L}\p{N}\s]", " ");
            var words = cleaned.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length >= 2 && !VietnameseStopWords.Contains(w))
                .Distinct()
                .ToList();

            return words.Take(8).ToList();
        }

        private static string BuildImageDataUrl(string base64, string? format)
        {
            var mime = string.Equals(format, "png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            return $"data:{mime};base64,{base64}";
        }

        private class ProductIdComparer : IEqualityComparer<ProductResponseDto>
        {
            public bool Equals(ProductResponseDto? x, ProductResponseDto? y) => x?.Id == y?.Id;
            public int GetHashCode(ProductResponseDto obj) => obj.Id.GetHashCode();
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
