using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Comment
{
    /// <summary>
    /// DTO for creating a comment
    /// </summary>
    public class CreateCommentRequestDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
