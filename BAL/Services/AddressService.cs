using BAL.DTOs.Address;
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Address service implementation with business logic
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly TechStoreContext _context;

        public AddressService(IAddressRepository addressRepository, TechStoreContext context)
        {
            _addressRepository = addressRepository;
            _context = context;
        }

        public async Task<IEnumerable<AddressResponseDto>> GetUserAddressesAsync(Guid userId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            return addresses.Select(MapToDto);
        }

        public async Task<AddressResponseDto?> GetAddressByIdAsync(Guid id, Guid userId)
        {
            var address = await _addressRepository.GetByIdAndUserIdAsync(id, userId);
            return address == null ? null : MapToDto(address);
        }

        public async Task<AddressResponseDto> CreateAddressAsync(Guid userId, CreateAddressRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Business rule: If this is set as primary, unset all other primary addresses
                if (request.IsPrimary)
                {
                    var existingPrimary = await _addressRepository.GetPrimaryAddressByUserIdAsync(userId);
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.UtcNow;
                        await _addressRepository.UpdateAsync(existingPrimary);
                    }
                }

                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RecipientName = request.RecipientName,
                    PhoneNumber = request.PhoneNumber,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    Ward = request.Ward,
                    District = request.District,
                    City = request.City,
                    IsPrimary = request.IsPrimary,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdAddress = await _addressRepository.AddAsync(address);
                await transaction.CommitAsync();
                return MapToDto(createdAddress);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AddressResponseDto?> UpdateAddressAsync(Guid id, Guid userId, UpdateAddressRequestDto request)
        {
            var address = await _addressRepository.GetByIdAndUserIdAsync(id, userId);
            if (address == null)
                return null;

            // Business rule: Only update provided fields
            if (request.RecipientName != null)
                address.RecipientName = request.RecipientName;

            if (request.PhoneNumber != null)
                address.PhoneNumber = request.PhoneNumber;

            if (request.AddressLine1 != null)
                address.AddressLine1 = request.AddressLine1;

            if (request.AddressLine2 != null)
                address.AddressLine2 = request.AddressLine2;

            if (request.Ward != null)
                address.Ward = request.Ward;

            if (request.District != null)
                address.District = request.District;

            if (request.City != null)
                address.City = request.City;

            address.UpdatedAt = DateTime.UtcNow;

            var updatedAddress = await _addressRepository.UpdateAsync(address);
            return MapToDto(updatedAddress);
        }

        public async Task<bool> DeleteAddressAsync(Guid id, Guid userId)
        {
            var address = await _addressRepository.GetByIdAndUserIdAsync(id, userId);
            if (address == null)
                return false;

            await _addressRepository.DeleteAsync(id);
            return true;
        }

        public async Task<AddressResponseDto> SetPrimaryAddressAsync(Guid id, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Business rule: Verify address belongs to user
                var address = await _addressRepository.GetByIdAndUserIdAsync(id, userId);
                if (address == null)
                    throw new InvalidOperationException("Address not found or does not belong to user");

                // Business rule: Set all other addresses to non-primary first
                var allUserAddresses = await _addressRepository.GetByUserIdAsync(userId);
                foreach (var addr in allUserAddresses.Where(a => a.IsPrimary && a.Id != id))
                {
                    addr.IsPrimary = false;
                    addr.UpdatedAt = DateTime.UtcNow;
                    await _addressRepository.UpdateAsync(addr);
                }

                // Set this address as primary
                address.IsPrimary = true;
                address.UpdatedAt = DateTime.UtcNow;
                var updatedAddress = await _addressRepository.UpdateAsync(address);

                await transaction.CommitAsync();
                return MapToDto(updatedAddress);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static AddressResponseDto MapToDto(Address address)
        {
            return new AddressResponseDto
            {
                Id = address.Id,
                UserId = address.UserId,
                RecipientName = address.RecipientName,
                PhoneNumber = address.PhoneNumber,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                Ward = address.Ward,
                District = address.District,
                City = address.City,
                IsPrimary = address.IsPrimary,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };
        }
    }
}
