using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Category repository interface
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync();
        Task<Category?> GetByNameAsync(string name);
    }
}
