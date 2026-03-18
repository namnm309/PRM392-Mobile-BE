using DAL.Models;

namespace DAL.Repositories
{
    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, bool? isActive = null);
        Task<ProductVariant?> GetByIdAndProductIdAsync(Guid id, Guid productId);
    }
}

