namespace BAL.DTOs.Dashboard
{
    /// <summary>
    /// Orders series response DTO for time series data
    /// </summary>
    public class OrdersSeriesResponseDto
    {
        public string Period { get; set; } = string.Empty; // Date string
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
