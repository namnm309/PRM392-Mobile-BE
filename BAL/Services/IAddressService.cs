using BAL.DTOs.Address;

namespace BAL.Services
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public interface IAddressService
    {
        Task<IEnumerable<AddressResponseDto>> GetUserAddressesAsync(Guid userId);
        Task<AddressResponseDto?> GetAddressByIdAsync(Guid id, Guid userId);
        Task<AddressResponseDto> CreateAddressAsync(Guid userId, CreateAddressRequestDto request);
        Task<AddressResponseDto?> UpdateAddressAsync(Guid id, Guid userId, UpdateAddressRequestDto request);
        Task<bool> DeleteAddressAsync(Guid id, Guid userId);
        Task<AddressResponseDto> SetPrimaryAddressAsync(Guid id, Guid userId);
    }
}
