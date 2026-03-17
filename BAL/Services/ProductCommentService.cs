using BAL.DTOs.ProductComment;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    public class ProductCommentService : IProductCommentService
    {
        private readonly IProductCommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;

        public ProductCommentService(
            IProductCommentRepository commentRepository,
            IProductRepository productRepository)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductCommentResponseDto>> GetCommentsByProductIdAsync(Guid productId)
        {
            var allComments = await _commentRepository.GetByProductIdAsync(productId);
            var commentList = allComments.ToList();

            var lookup = commentList.ToLookup(c => c.ParentId);

            var topLevel = commentList.Where(c => c.ParentId == null).ToList();

            return topLevel.Select(c => BuildTree(c, lookup));
        }

        public async Task<ProductCommentResponseDto> CreateCommentAsync(Guid userId, CreateProductCommentRequestDto request)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            if (request.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value);
                if (parentComment == null)
                {
                    throw new InvalidOperationException("Parent comment not found");
                }

                if (parentComment.ProductId != request.ProductId)
                {
                    throw new InvalidOperationException("Parent comment does not belong to this product");
                }
            }

            var comment = new ProductComment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                ParentId = request.ParentId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _commentRepository.AddAsync(comment);
            var withDetails = await _commentRepository.GetByIdWithRepliesAsync(created.Id);
            return MapToDto(withDetails!);
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return false;

            return await _commentRepository.DeleteAsync(commentId);
        }

        private static ProductCommentResponseDto BuildTree(
            ProductComment comment,
            ILookup<Guid?, ProductComment> lookup)
        {
            var dto = MapToDto(comment);
            var children = lookup[comment.Id];
            dto.Replies = children
                .OrderBy(c => c.CreatedAt)
                .Select(c => BuildTree(c, lookup))
                .ToList();
            return dto;
        }

        private static ProductCommentResponseDto MapToDto(ProductComment comment)
        {
            return new ProductCommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = comment.User?.FullName ?? "Unknown",
                UserAvatarUrl = comment.User?.AvatarUrl,
                ProductId = comment.ProductId,
                ParentId = comment.ParentId,
                Content = comment.Content,
                Replies = new List<ProductCommentResponseDto>(),
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }
    }
}
