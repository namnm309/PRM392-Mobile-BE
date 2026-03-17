using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Review
{
    public class CreateReviewReplyRequestDto
    {
        [Required(ErrorMessage = "Reply content is required")]
        [MaxLength(1000, ErrorMessage = "Reply content cannot exceed 1000 characters")]
        public string ReplyContent { get; set; } = string.Empty;
    }
}
