using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Category repository interface
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync(bool? isActive = null);
        Task<Category?> GetByNameAsync(string name);
    }
}
