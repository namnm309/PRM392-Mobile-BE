using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Address
{
    /// <summary>
    /// DTO for creating a new address
    /// </summary>
    public class CreateAddressRequestDto
    {
        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(200, ErrorMessage = "Recipient name cannot exceed 200 characters")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line 1 is required")]
        [MaxLength(500, ErrorMessage = "Address line 1 cannot exceed 500 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Address line 2 cannot exceed 500 characters")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Ward is required")]
        [MaxLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required")]
        [MaxLength(100, ErrorMessage = "District cannot exceed 100 characters")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
    }
}
