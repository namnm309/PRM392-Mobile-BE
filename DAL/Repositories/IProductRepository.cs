using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Product repository interface
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByIdWithTrackingAsync(Guid id);
        Task<Product?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetAllWithFiltersAsync(bool? isActive = null, Guid? categoryId = null, Guid? brandId = null);
        Task<IEnumerable<Product>> SearchByNameAsync(string name, bool? isActive = null);
        Task<IEnumerable<Product>> SearchByBrandIdAsync(Guid brandId, bool? isActive = null);
        Task<IEnumerable<Product>> GetSimilarProductsAsync(Guid productId, int limit = 5);
        Task<bool> IsProductAvailableAsync(Guid productId, int quantity);
    }
}
