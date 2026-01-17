namespace BAL.DTOs.Product
{
    /// <summary>
    /// Search product request DTO
    /// </summary>
    public class SearchProductRequestDto
    {
        public string? Name { get; set; }
        public Guid? BrandId { get; set; }
        public bool? IsActive { get; set; }
    }
}
