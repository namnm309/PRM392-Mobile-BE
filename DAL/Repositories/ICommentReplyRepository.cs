using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// CommentReply repository interface
    /// </summary>
    public interface ICommentReplyRepository : IRepository<CommentReply>
    {
        Task<CommentReply?> GetByCommentIdAsync(Guid commentId);
        Task<bool> HasReplyAsync(Guid commentId);
    }
}
