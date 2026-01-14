using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Address
{
    /// <summary>
    /// DTO for updating address
    /// </summary>
    public class UpdateAddressRequestDto
    {
        [MaxLength(200, ErrorMessage = "Recipient name cannot exceed 200 characters")]
        public string? RecipientName { get; set; }

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address line 1 cannot exceed 500 characters")]
        public string? AddressLine1 { get; set; }

        [MaxLength(500, ErrorMessage = "Address line 2 cannot exceed 500 characters")]
        public string? AddressLine2 { get; set; }

        [MaxLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
        public string? Ward { get; set; }

        [MaxLength(100, ErrorMessage = "District cannot exceed 100 characters")]
        public string? District { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }
    }
}
