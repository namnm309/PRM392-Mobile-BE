using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ReviewReplyRepository : Repository<ReviewReply>, IReviewReplyRepository
    {
        public ReviewReplyRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<ReviewReply?> GetByReviewIdAsync(Guid reviewId)
        {
            return await _dbSet
                .Include(rr => rr.Staff)
                .FirstOrDefaultAsync(rr => rr.ReviewId == reviewId);
        }

        public async Task<bool> HasReplyAsync(Guid reviewId)
        {
            return await _dbSet.AnyAsync(rr => rr.ReviewId == reviewId);
        }
    }
}
