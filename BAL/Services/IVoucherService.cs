using BAL.DTOs.Voucher;

namespace BAL.Services
{
    /// <summary>
    /// Voucher service interface
    /// </summary>
    public interface IVoucherService
    {
        Task<VoucherResponseDto?> GetVoucherByCodeAsync(string code);
        Task<VoucherBreakdownResponseDto> ApplyVoucherAsync(Guid userId, string code, List<Guid> cartItemIds);
    }
}
