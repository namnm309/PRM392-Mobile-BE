using BAL.DTOs.Voucher;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// Voucher service implementation with business logic
    /// </summary>
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public VoucherService(
            IVoucherRepository voucherRepository,
            IVoucherUsageRepository voucherUsageRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository)
        {
            _voucherRepository = voucherRepository;
            _voucherUsageRepository = voucherUsageRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<VoucherResponseDto?> GetVoucherByCodeAsync(string code)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(code);
            if (voucher == null)
                return null;

            var usageCount = await _voucherRepository.GetUsageCountAsync(voucher.Id);
            var now = DateTime.UtcNow;
            var isValid = voucher.IsActive 
                        && voucher.StartTime <= now 
                        && voucher.EndTime >= now
                        && (voucher.TotalUsageLimit == 0 || usageCount < voucher.TotalUsageLimit);

            return new VoucherResponseDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                DiscountType = voucher.DiscountType,
                Value = voucher.Value,
                StartTime = voucher.StartTime,
                EndTime = voucher.EndTime,
                MinOrderValue = voucher.MinOrderValue,
                TotalUsageLimit = voucher.TotalUsageLimit,
                PerUserLimit = voucher.PerUserLimit,
                IsActive = voucher.IsActive,
                IsValid = isValid,
                CurrentUsage = usageCount
            };
        }

        public async Task<VoucherBreakdownResponseDto> ApplyVoucherAsync(Guid userId, string code, List<Guid> cartItemIds)
        {
            // Get voucher
            var voucher = await _voucherRepository.GetByCodeAsync(code);
            if (voucher == null)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "Voucher not found"
                };
            }

            var now = DateTime.UtcNow;

            // Business rule: Check voucher is active and within time range
            if (!voucher.IsActive)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "Voucher is not active"
                };
            }

            if (voucher.StartTime > now || voucher.EndTime < now)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "Voucher is not valid at this time"
                };
            }

            // Business rule: Check total usage limit
            var totalUsage = await _voucherRepository.GetUsageCountAsync(voucher.Id);
            if (voucher.TotalUsageLimit > 0 && totalUsage >= voucher.TotalUsageLimit)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "Voucher has reached its usage limit"
                };
            }

            // Business rule: Check per user limit
            var userUsage = await _voucherRepository.GetUsageCountByUserAsync(voucher.Id, userId);
            if (userUsage >= voucher.PerUserLimit)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "You have reached the usage limit for this voucher"
                };
            }

            // Get cart items
            var cartItems = await _cartItemRepository.GetByUserIdAsync(userId);
            var selectedItems = cartItems.Where(ci => cartItemIds.Contains(ci.Id)).ToList();

            if (!selectedItems.Any())
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "No items selected"
                };
            }

            // Business rule: Calculate eligible items (exclude sale items and noVoucherTag items)
            var eligibleItems = new List<CartItem>();
            var ineligibleItems = new List<string>();

            foreach (var item in selectedItems)
            {
                var product = item.Product ?? await _productRepository.GetByIdAsync(item.ProductId);
                
                if (product == null || !product.IsActive)
                {
                    ineligibleItems.Add($"{item.Product?.Name ?? "Unknown"} - Product not available");
                    continue;
                }

                // Check if product is on sale or has discount
                bool isOnSale = product.IsOnSale || (product.DiscountPrice.HasValue && product.DiscountPrice < product.Price);
                
                if (isOnSale || product.NoVoucherTag)
                {
                    ineligibleItems.Add($"{product.Name} - {(isOnSale ? "Product is on sale" : "Voucher not applicable for this product")}");
                    continue;
                }

                eligibleItems.Add(item);
            }

            if (!eligibleItems.Any())
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = "No eligible items for voucher. All items are on sale or have voucher restrictions.",
                    IneligibleItems = ineligibleItems
                };
            }

            // Calculate subtotal of eligible items
            var subtotalEligible = eligibleItems.Sum(item => item.UnitPriceSnapshot * item.Quantity);

            // Business rule: Check minimum order value
            if (subtotalEligible < voucher.MinOrderValue)
            {
                return new VoucherBreakdownResponseDto
                {
                    ErrorMessage = $"Minimum order value of {voucher.MinOrderValue:C} is required. Current eligible subtotal: {subtotalEligible:C}"
                };
            }

            // Calculate discount
            decimal discountAmount = 0;
            if (voucher.DiscountType == "Percent")
            {
                discountAmount = subtotalEligible * (voucher.Value / 100);
            }
            else if (voucher.DiscountType == "Fixed")
            {
                discountAmount = Math.Min(voucher.Value, subtotalEligible);
            }

            var finalTotal = subtotalEligible - discountAmount;

            return new VoucherBreakdownResponseDto
            {
                SubtotalEligible = subtotalEligible,
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal,
                VoucherCode = voucher.Code,
                IneligibleItems = ineligibleItems
            };
        }
    }
}
