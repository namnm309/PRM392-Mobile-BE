namespace BAL.DTOs.Membership
{
    public class MembershipTierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int MaxPoints { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? Benefits { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UserMembershipDto
    {
        public int TotalPoints { get; set; }
        public int AvailablePoints { get; set; }
        public MembershipTierDto? CurrentTier { get; set; }
        public MembershipTierDto? NextTier { get; set; }
        public int PointsToNextTier { get; set; }
    }

    public class PointTransactionDto
    {
        public Guid Id { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PointHistoryResponse
    {
        public int TotalPoints { get; set; }
        public int AvailablePoints { get; set; }
        public List<PointTransactionDto> Transactions { get; set; } = new();
    }

    public class CreateMembershipTierRequest
    {
        public string Name { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int MaxPoints { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? Benefits { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateMembershipTierRequest
    {
        public string? Name { get; set; }
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string? Benefits { get; set; }
        public string? IconUrl { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AddPointsRequest
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } = "Earned";
        public Guid? OrderId { get; set; }
        public string? Description { get; set; }
    }
}
