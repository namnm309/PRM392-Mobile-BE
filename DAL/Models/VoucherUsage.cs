using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// VoucherUsage entity - Lịch sử sử dụng voucher
    /// </summary>
    [Table("tbl_voucher_usages")]
    public class VoucherUsage
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("voucher_id")]
        public Guid VoucherId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("order_id")]
        public Guid OrderId { get; set; }

        [Required]
        [Column("discount_amount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column("used_at")]
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}
