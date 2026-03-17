using BAL.DTOs.ProductComment;

namespace BAL.Services
{
    public interface IProductCommentService
    {
        Task<IEnumerable<ProductCommentResponseDto>> GetCommentsByProductIdAsync(Guid productId);
        Task<ProductCommentResponseDto> CreateCommentAsync(Guid userId, CreateProductCommentRequestDto request);
        Task<bool> DeleteCommentAsync(Guid commentId);
    }
}
