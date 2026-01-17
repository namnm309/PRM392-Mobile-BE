using BAL.DTOs.Comment;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// Comment service implementation with business logic
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentReplyRepository _commentReplyRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public CommentService(
            ICommentRepository commentRepository,
            ICommentReplyRepository commentReplyRepository,
            IOrderItemRepository orderItemRepository)
        {
            _commentRepository = commentRepository;
            _commentReplyRepository = commentReplyRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByProductIdAsync(Guid productId)
        {
            var comments = await _commentRepository.GetByProductIdAsync(productId);
            return comments.Select(MapToDto);
        }

        public async Task<CommentResponseDto> CreateCommentAsync(Guid userId, CreateCommentRequestDto request)
        {
            // Business rule: Check if user has purchased and received product (SUCCESS status)
            var hasPurchased = await _orderItemRepository.HasUserPurchasedProductAsync(userId, request.ProductId);
            if (!hasPurchased)
            {
                throw new InvalidOperationException("You can only comment on products you have successfully received");
            }

            // Business rule: Check if user has already commented (enforced by unique constraint)
            var existingComment = await _commentRepository.GetByUserIdAndProductIdAsync(userId, request.ProductId);
            if (existingComment != null)
            {
                throw new InvalidOperationException("You have already commented on this product");
            }

            // Business rule: Validate rating
            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.AddAsync(comment);
            var commentWithDetails = await _commentRepository.GetByIdWithReplyAsync(createdComment.Id);
            return MapToDto(commentWithDetails!);
        }

        public async Task<CommentReplyResponseDto> ReplyToCommentAsync(Guid commentId, Guid staffId, CreateCommentReplyRequestDto request)
        {
            // Business rule: Check if comment exists
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                throw new InvalidOperationException("Comment not found");
            }

            // Business rule: Check if already replied (enforced by unique constraint on commentId)
            var existingReply = await _commentReplyRepository.GetByCommentIdAsync(commentId);
            if (existingReply != null)
            {
                throw new InvalidOperationException("This comment has already been replied to");
            }

            var reply = new CommentReply
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                StaffId = staffId,
                ReplyContent = request.ReplyContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdReply = await _commentReplyRepository.AddAsync(reply);
            var replyWithDetails = await _commentReplyRepository.GetByCommentIdAsync(commentId);
            return MapReplyToDto(replyWithDetails!);
        }

        private static CommentResponseDto MapToDto(Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = comment.User?.FullName ?? "Unknown",
                UserAvatarUrl = comment.User?.AvatarUrl,
                ProductId = comment.ProductId,
                Rating = comment.Rating,
                Content = comment.Content,
                Reply = comment.Reply != null ? MapReplyToDto(comment.Reply) : null,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        private static CommentReplyResponseDto MapReplyToDto(CommentReply reply)
        {
            return new CommentReplyResponseDto
            {
                Id = reply.Id,
                CommentId = reply.CommentId,
                StaffId = reply.StaffId,
                StaffName = reply.Staff?.FullName ?? "Staff",
                ReplyContent = reply.ReplyContent,
                CreatedAt = reply.CreatedAt
            };
        }
    }
}
