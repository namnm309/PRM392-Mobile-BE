namespace BAL.DTOs.Product
{
    public class ProductVariantResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? Sku { get; set; }
        public string? VariantName { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public int? RamGb { get; set; }
        public int? StorageGb { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

