using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Product repository interface
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByIdWithTrackingAsync(Guid id);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<bool> IsProductAvailableAsync(Guid productId, int quantity);
    }
}
