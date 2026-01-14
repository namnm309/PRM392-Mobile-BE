namespace BAL.DTOs.ProductImage
{
    /// <summary>
    /// ProductImage response DTO
    /// </summary>
    public class ProductImageResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
