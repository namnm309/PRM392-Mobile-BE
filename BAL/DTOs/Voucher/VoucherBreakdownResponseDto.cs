namespace BAL.DTOs.Voucher
{
    /// <summary>
    /// Voucher breakdown response DTO
    /// </summary>
    public class VoucherBreakdownResponseDto
    {
        public decimal SubtotalEligible { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public List<string> IneligibleItems { get; set; } = new();
    }
}
