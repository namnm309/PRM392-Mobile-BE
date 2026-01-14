using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Brand repository interface
    /// </summary>
    public interface IBrandRepository : IRepository<Brand>
    {
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
        Task<Brand?> GetByNameAsync(string name);
    }
}
