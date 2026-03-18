using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("tbl_product_variants")]
    public class ProductVariant
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("product_id")]
        public Guid ProductId { get; set; }

        [Column("sku")]
        [MaxLength(100)]
        public string? Sku { get; set; }

        [Column("variant_name")]
        [MaxLength(200)]
        public string? VariantName { get; set; }

        [Required]
        [Column("color_name")]
        [MaxLength(100)]
        public string ColorName { get; set; } = string.Empty;

        [Column("color_hex")]
        [MaxLength(20)]
        public string? ColorHex { get; set; }

        [Column("ram_gb")]
        public int? RamGb { get; set; }

        [Column("storage_gb")]
        public int? StorageGb { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column("discount_price", TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; } = 0;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}

