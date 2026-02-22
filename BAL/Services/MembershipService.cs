using BAL.DTOs.Membership;
using DAL.Models;
using DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipTierRepository _tierRepository;
        private readonly IPointTransactionRepository _pointRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<MembershipService> _logger;
        private const int PointsPerThousand = 1;

        public MembershipService(
            IMembershipTierRepository tierRepository,
            IPointTransactionRepository pointRepository,
            IUserRepository userRepository,
            ILogger<MembershipService> logger)
        {
            _tierRepository = tierRepository;
            _pointRepository = pointRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<MembershipTierDto>> GetAllTiersAsync()
        {
            var tiers = await _tierRepository.GetAllActiveAsync();
            return tiers.Select(MapTierToDto);
        }

        public async Task<MembershipTierDto?> GetTierByIdAsync(Guid tierId)
        {
            var tier = await _tierRepository.GetByIdAsync(tierId);
            return tier != null ? MapTierToDto(tier) : null;
        }

        public async Task<UserMembershipDto> GetUserMembershipAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var totalEarned = await _pointRepository.GetTotalPointsEarnedAsync(userId);
            var totalRedeemed = await _pointRepository.GetTotalPointsRedeemedAsync(userId);
            var availablePoints = totalEarned - totalRedeemed;

            var currentTier = await _tierRepository.GetTierByPointsAsync(totalEarned);
            var allTiers = await _tierRepository.GetAllActiveAsync();
            var nextTier = allTiers.Where(t => t.MinPoints > totalEarned).OrderBy(t => t.MinPoints).FirstOrDefault();
            var pointsToNextTier = nextTier != null ? nextTier.MinPoints - totalEarned : 0;

            return new UserMembershipDto
            {
                TotalPoints = totalEarned,
                AvailablePoints = availablePoints,
                CurrentTier = currentTier != null ? MapTierToDto(currentTier) : null,
                NextTier = nextTier != null ? MapTierToDto(nextTier) : null,
                PointsToNextTier = pointsToNextTier
            };
        }

        public async Task<PointHistoryResponse> GetUserPointHistoryAsync(Guid userId, int limit = 20)
        {
            var totalEarned = await _pointRepository.GetTotalPointsEarnedAsync(userId);
            var totalRedeemed = await _pointRepository.GetTotalPointsRedeemedAsync(userId);
            var transactions = await _pointRepository.GetByUserIdAsync(userId, limit);

            return new PointHistoryResponse
            {
                TotalPoints = totalEarned,
                AvailablePoints = totalEarned - totalRedeemed,
                Transactions = transactions.Select(MapTransactionToDto).ToList()
            };
        }

        public async Task<MembershipTierDto> CreateTierAsync(CreateMembershipTierRequest request)
        {
            var tier = new MembershipTier
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                MinPoints = request.MinPoints,
                MaxPoints = request.MaxPoints,
                DiscountPercent = request.DiscountPercent,
                Benefits = request.Benefits,
                IconUrl = request.IconUrl,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tierRepository.AddAsync(tier);
            _logger.LogInformation("Created membership tier: {TierName}", tier.Name);
            return MapTierToDto(tier);
        }

        public async Task<MembershipTierDto?> UpdateTierAsync(Guid tierId, UpdateMembershipTierRequest request)
        {
            var tier = await _tierRepository.GetByIdAsync(tierId);
            if (tier == null) return null;

            if (request.Name != null) tier.Name = request.Name;
            if (request.MinPoints.HasValue) tier.MinPoints = request.MinPoints.Value;
            if (request.MaxPoints.HasValue) tier.MaxPoints = request.MaxPoints.Value;
            if (request.DiscountPercent.HasValue) tier.DiscountPercent = request.DiscountPercent.Value;
            if (request.Benefits != null) tier.Benefits = request.Benefits;
            if (request.IconUrl != null) tier.IconUrl = request.IconUrl;
            if (request.DisplayOrder.HasValue) tier.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive.HasValue) tier.IsActive = request.IsActive.Value;
            tier.UpdatedAt = DateTime.UtcNow;

            await _tierRepository.UpdateAsync(tier);
            _logger.LogInformation("Updated membership tier: {TierId}", tierId);
            return MapTierToDto(tier);
        }

        public async Task<bool> DeleteTierAsync(Guid tierId)
        {
            var tier = await _tierRepository.GetByIdAsync(tierId);
            if (tier == null) return false;

            await _tierRepository.DeleteAsync(tierId);
            _logger.LogInformation("Deleted membership tier: {TierId}", tierId);
            return true;
        }

        public async Task<PointTransactionDto> AddPointsAsync(AddPointsRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");

            var transaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Points = request.Points,
                Type = request.Type,
                OrderId = request.OrderId,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _pointRepository.AddAsync(transaction);
            _logger.LogInformation("Added {Points} points to user {UserId}", request.Points, request.UserId);
            return MapTransactionToDto(transaction);
        }

        public async Task AddPointsFromOrderAsync(Guid userId, Guid orderId, decimal orderTotal)
        {
            var points = (int)(orderTotal / 1000 * PointsPerThousand);
            if (points <= 0) return;

            var transaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Points = points,
                Type = "Earned",
                OrderId = orderId,
                Description = $"Earned from order #{orderId.ToString()[..8]}",
                CreatedAt = DateTime.UtcNow
            };

            await _pointRepository.AddAsync(transaction);
            _logger.LogInformation("User {UserId} earned {Points} points from order {OrderId}", userId, points, orderId);
        }

        private MembershipTierDto MapTierToDto(MembershipTier tier) => new()
        {
            Id = tier.Id,
            Name = tier.Name,
            MinPoints = tier.MinPoints,
            MaxPoints = tier.MaxPoints,
            DiscountPercent = tier.DiscountPercent,
            Benefits = tier.Benefits,
            IconUrl = tier.IconUrl,
            DisplayOrder = tier.DisplayOrder
        };

        private PointTransactionDto MapTransactionToDto(PointTransaction t) => new()
        {
            Id = t.Id,
            Points = t.Points,
            Type = t.Type,
            OrderId = t.OrderId,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
    }
}
