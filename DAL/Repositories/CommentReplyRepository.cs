using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// CommentReply repository implementation
    /// </summary>
    public class CommentReplyRepository : Repository<CommentReply>, ICommentReplyRepository
    {
        public CommentReplyRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<CommentReply?> GetByCommentIdAsync(Guid commentId)
        {
            return await _dbSet
                .Include(cr => cr.Staff)
                .FirstOrDefaultAsync(cr => cr.CommentId == commentId);
        }

        public async Task<bool> HasReplyAsync(Guid commentId)
        {
            return await _dbSet.AnyAsync(cr => cr.CommentId == commentId);
        }
    }
}
