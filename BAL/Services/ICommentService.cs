using BAL.DTOs.Comment;

namespace BAL.Services
{
    /// <summary>
    /// Comment service interface
    /// </summary>
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetCommentsByProductIdAsync(Guid productId);
        Task<CommentResponseDto> CreateCommentAsync(Guid userId, CreateCommentRequestDto request);
        Task<CommentReplyResponseDto> ReplyToCommentAsync(Guid commentId, Guid staffId, CreateCommentReplyRequestDto request);
    }
}
