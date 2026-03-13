using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// Mapping entity between Category and Brand (many-to-many).
    /// </summary>
    [Table("tbl_category_brands")]
    public class CategoryBrand
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey(nameof(BrandId))]
        public virtual Brand Brand { get; set; } = null!;
    }
}

