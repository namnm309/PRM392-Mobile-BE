namespace BAL.DTOs.Address
{
    /// <summary>
    /// Address response DTO
    /// </summary>
    public class AddressResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public string? WardCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AddressNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
