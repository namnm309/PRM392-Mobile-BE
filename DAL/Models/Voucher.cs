using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Voucher entity - Mã khuyến mãi
    /// </summary>
    [Table("tbl_vouchers")]
    public class Voucher
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("discount_type")]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "Percent"; // Percent or Fixed

        [Required]
        [Column("value", TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [Required]
        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Required]
        [Column("min_order_value", TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0;

        [Required]
        [Column("total_usage_limit")]
        public int TotalUsageLimit { get; set; } = 0; // 0 = unlimited

        [Required]
        [Column("per_user_limit")]
        public int PerUserLimit { get; set; } = 1;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    }
}
