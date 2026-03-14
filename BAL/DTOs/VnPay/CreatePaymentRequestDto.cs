using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.VnPay
{
    public class CreatePaymentRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }
    }
}
