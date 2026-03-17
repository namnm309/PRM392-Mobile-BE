using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.ProductComment
{
    public class CreateProductCommentRequestDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// If provided, this comment is a reply to the specified comment.
        /// </summary>
        public Guid? ParentId { get; set; }
    }
}
