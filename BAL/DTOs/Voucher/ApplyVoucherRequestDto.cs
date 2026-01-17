using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Voucher
{
    /// <summary>
    /// DTO for applying voucher at checkout
    /// </summary>
    public class ApplyVoucherRequestDto
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [MaxLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;
    }
}
