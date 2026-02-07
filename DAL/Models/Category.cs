using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Category entity - Danh mục sản phẩm
    /// </summary>
    [Table("tbl_categories")]
    public class Category
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

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("parent_id")]
        public Guid? ParentId { get; set; }

        [Column("image_url")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("is_hot")]
        public bool IsHot { get; set; } = false;

        // Navigation Properties
        [ForeignKey("ParentId")]
        public virtual Category? Parent { get; set; }

        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
