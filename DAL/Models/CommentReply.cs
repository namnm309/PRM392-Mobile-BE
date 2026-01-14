using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    /// <summary>
    /// CommentReply entity - Reply của staff cho comment
    /// </summary>
    [Table("tbl_comment_replies")]
    public class CommentReply
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("comment_id")]
        public Guid CommentId { get; set; }

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

        // Navigation Properties
        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; } = null!;

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; } = null!;
    }
}
