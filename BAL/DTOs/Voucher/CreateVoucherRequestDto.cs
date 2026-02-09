using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Voucher
{
    /// <summary>
    /// Create voucher request DTO
    /// </summary>
    public class CreateVoucherRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "Percent"; // Percent, Amount, or Fixed

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal Value { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinOrderValue must be greater than or equal to 0")]
        public decimal MinOrderValue { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "MaxDiscountValue must be greater than or equal to 0")]
        public decimal? MaxDiscountValue { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
