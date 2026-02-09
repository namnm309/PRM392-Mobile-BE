using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Voucher
{
    /// <summary>
    /// Update voucher request DTO
    /// </summary>
    public class UpdateVoucherRequestDto
    {
        [MaxLength(50)]
        public string? Code { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? DiscountType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal? Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MinOrderValue must be greater than or equal to 0")]
        public decimal? MinOrderValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaxDiscountValue must be greater than or equal to 0")]
        public decimal? MaxDiscountValue { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool? IsActive { get; set; }
    }
}
