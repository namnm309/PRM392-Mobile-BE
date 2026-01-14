using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Address repository interface
    /// </summary>
    public interface IAddressRepository : IRepository<Address>
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId);
        Task<Address?> GetPrimaryAddressByUserIdAsync(Guid userId);
        Task<Address?> GetByIdAndUserIdAsync(Guid id, Guid userId);
        Task<bool> HasPrimaryAddressAsync(Guid userId);
    }
}
