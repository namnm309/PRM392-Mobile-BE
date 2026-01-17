namespace BAL.DTOs.Comment
{
    /// <summary>
    /// Comment response DTO
    /// </summary>
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommentReplyResponseDto? Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
