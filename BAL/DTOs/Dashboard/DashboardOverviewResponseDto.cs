namespace BAL.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard overview response DTO
    /// </summary>
    public class DashboardOverviewResponseDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int SuccessOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int NewUsers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
