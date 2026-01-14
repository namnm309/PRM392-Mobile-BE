using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// ProductImage repository interface
    /// </summary>
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductImage>> GetByProductIdAndTypeAsync(Guid productId, string imageType);
        Task<bool> DeleteByProductIdAsync(Guid productId);
        Task<ProductImage?> GetMainImageByProductIdAsync(Guid productId);
    }
}
