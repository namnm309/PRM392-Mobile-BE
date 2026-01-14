namespace BAL.DTOs.Voucher
{
    /// <summary>
    /// Voucher response DTO
    /// </summary>
    public class VoucherResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal MinOrderValue { get; set; }
        public int TotalUsageLimit { get; set; }
        public int PerUserLimit { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
        public int CurrentUsage { get; set; }
    }
}
