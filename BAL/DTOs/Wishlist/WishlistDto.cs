namespace BAL.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductDiscountPrice { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsAvailable { get; set; }
        public int Stock { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class AddToWishlistRequest
    {
        public Guid ProductId { get; set; }
    }

    public class WishlistStatusDto
    {
        public bool IsInWishlist { get; set; }
        public DateTime? AddedAt { get; set; }
    }
}
