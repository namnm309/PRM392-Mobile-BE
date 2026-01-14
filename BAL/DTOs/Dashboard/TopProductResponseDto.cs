namespace BAL.DTOs.Dashboard
{
    /// <summary>
    /// Top product response DTO
    /// </summary>
    public class TopProductResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}
