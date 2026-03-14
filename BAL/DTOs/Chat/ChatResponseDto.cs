namespace BAL.DTOs.Chat
{
    /// <summary>
    /// Response từ AI chat bot
    /// </summary>
    public class ChatResponseDto
    {
        public string Content { get; set; } = string.Empty;

        public ChatUsageDto? Usage { get; set; }

        /// <summary>
        /// ID sản phẩm chính được tư vấn (từ DB context) - dùng để hiển thị nút So sánh/Yêu thích
        /// </summary>
        public Guid? PrimaryProductId { get; set; }
    }

    public class ChatUsageDto
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
