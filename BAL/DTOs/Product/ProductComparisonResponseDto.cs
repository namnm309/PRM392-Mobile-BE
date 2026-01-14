namespace BAL.DTOs.Product
{
    /// <summary>
    /// Product comparison response DTO
    /// </summary>
    public class ProductComparisonResponseDto
    {
        public ProductResponseDto OriginalProduct { get; set; } = null!;
        public List<ProductResponseDto> SimilarProducts { get; set; } = new();
    }
}
