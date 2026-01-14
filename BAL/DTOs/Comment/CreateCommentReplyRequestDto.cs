using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Comment
{
    /// <summary>
    /// DTO for creating a comment reply (Staff only)
    /// </summary>
    public class CreateCommentReplyRequestDto
    {
        [Required(ErrorMessage = "Reply content is required")]
        [MaxLength(1000, ErrorMessage = "Reply content cannot exceed 1000 characters")]
        public string ReplyContent { get; set; } = string.Empty;
    }
}
