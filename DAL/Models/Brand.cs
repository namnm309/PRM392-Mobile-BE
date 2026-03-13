using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Brand entity - Thương hiệu sản phẩm
    /// </summary>
    [Table("tbl_brands")]
    public class Brand
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("image_url")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<CategoryBrand> CategoryBrands { get; set; } = new List<CategoryBrand>();
    }
}
