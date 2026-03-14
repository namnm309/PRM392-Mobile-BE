using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Address entity - Địa chỉ của user
    /// </summary>
    [Table("tbl_addresses")]
    public class Address
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("recipient_name")]
        [MaxLength(200)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [Column("phone_number")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Column("address_line1")]
        [MaxLength(500)]
        public string AddressLine1 { get; set; } = string.Empty;

        [Column("address_line2")]
        [MaxLength(500)]
        public string? AddressLine2 { get; set; }

        [Required]
        [Column("ward")]
        [MaxLength(100)]
        public string Ward { get; set; } = string.Empty;

        [Required]
        [Column("district")]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required]
        [Column("city")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("province_id")]
        public int? ProvinceId { get; set; }

        [Column("district_id")]
        public int? DistrictId { get; set; }

        [Column("ward_code")]
        [MaxLength(20)]
        public string? WardCode { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("address_note")]
        [MaxLength(500)]
        public string? AddressNote { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
