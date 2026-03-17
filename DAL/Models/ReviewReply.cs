using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("tbl_review_replies")]
    public class ReviewReply
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("review_id")]
        public Guid ReviewId { get; set; }

        [Required]
        [Column("staff_id")]
        public Guid StaffId { get; set; }

        [Required]
        [Column("reply_content", TypeName = "text")]
        public string ReplyContent { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; } = null!;

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; } = null!;
    }
}
