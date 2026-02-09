using BAL.DTOs.Voucher;

namespace BAL.Services
{
    /// <summary>
    /// Voucher service interface
    /// </summary>
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherResponseDto>> GetAllVouchersAsync(string? code = null, string? name = null, bool? isActive = null);
        Task<VoucherResponseDto?> GetVoucherByIdAsync(Guid id);
        Task<VoucherResponseDto?> GetVoucherByCodeAsync(string code);
        Task<VoucherResponseDto> CreateVoucherAsync(CreateVoucherRequestDto request);
        Task<VoucherResponseDto?> UpdateVoucherAsync(Guid id, UpdateVoucherRequestDto request);
        Task<bool> DeleteVoucherAsync(Guid id);
        Task<VoucherResponseDto?> ToggleActiveAsync(Guid id);
        Task<VoucherBreakdownResponseDto> ApplyVoucherAsync(Guid userId, string code, List<Guid> cartItemIds);
    }
}
