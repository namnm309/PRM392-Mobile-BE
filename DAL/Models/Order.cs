using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Order entity - Đơn hàng
    /// </summary>
    [Table("tbl_orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("address_id")]
        public Guid AddressId { get; set; }

        [Required]
        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, SUCCESS, Cancelled

        [Required]
        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column("discount_amount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("voucher_id")]
        public Guid? VoucherId { get; set; }

        [Required]
        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("notes")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
