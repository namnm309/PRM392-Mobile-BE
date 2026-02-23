using BAL.DTOs.Membership;

namespace BAL.Services
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipTierDto>> GetAllTiersAsync();
        Task<MembershipTierDto?> GetTierByIdAsync(Guid tierId);
        Task<UserMembershipDto> GetUserMembershipAsync(Guid userId);
        Task<PointHistoryResponse> GetUserPointHistoryAsync(Guid userId, int limit = 20);
        Task<MembershipTierDto> CreateTierAsync(CreateMembershipTierRequest request);
        Task<MembershipTierDto?> UpdateTierAsync(Guid tierId, UpdateMembershipTierRequest request);
        Task<bool> DeleteTierAsync(Guid tierId);
        Task<PointTransactionDto> AddPointsAsync(AddPointsRequest request);
        Task AddPointsFromOrderAsync(Guid userId, Guid orderId, decimal orderTotal);
    }
}
