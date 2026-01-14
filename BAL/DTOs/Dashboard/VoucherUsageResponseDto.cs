namespace BAL.DTOs.Dashboard
{
    /// <summary>
    /// Voucher usage response DTO
    /// </summary>
    public class VoucherUsageResponseDto
    {
        public Guid VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public int TotalUsageLimit { get; set; }
        public decimal UsagePercentage { get; set; }
    }
}
