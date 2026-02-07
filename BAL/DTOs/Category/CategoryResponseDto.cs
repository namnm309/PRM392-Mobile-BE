namespace BAL.DTOs.Category
{
    /// <summary>
    /// Category response DTO
    /// </summary>
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHot { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryResponseDto> Children { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
