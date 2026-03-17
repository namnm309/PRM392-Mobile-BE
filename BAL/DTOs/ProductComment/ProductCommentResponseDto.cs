namespace BAL.DTOs.ProductComment
{
    public class ProductCommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ParentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<ProductCommentResponseDto> Replies { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
