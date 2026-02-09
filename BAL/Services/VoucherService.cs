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

        public async Task<IEnumerable<VoucherResponseDto>> GetAllVouchersAsync(string? code = null, string? name = null, bool? isActive = null)
        {
            var vouchers = await _voucherRepository.GetAllWithFiltersAsync(code, name, isActive);
            var result = new List<VoucherResponseDto>();

            foreach (var voucher in vouchers)
            {
                var usageCount = await _voucherRepository.GetUsageCountAsync(voucher.Id);
                var now = DateTime.UtcNow;
                var isValid = voucher.IsActive
                            && voucher.StartTime <= now
                            && voucher.EndTime >= now
                            && (voucher.TotalUsageLimit == 0 || usageCount < voucher.TotalUsageLimit);

                result.Add(MapToDto(voucher, isValid, usageCount));
            }

            return result;
        }

        public async Task<VoucherResponseDto?> GetVoucherByIdAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null)
                return null;

            var usageCount = await _voucherRepository.GetUsageCountAsync(voucher.Id);
            var now = DateTime.UtcNow;
            var isValid = voucher.IsActive
                        && voucher.StartTime <= now
                        && voucher.EndTime >= now
                        && (voucher.TotalUsageLimit == 0 || usageCount < voucher.TotalUsageLimit);

            return MapToDto(voucher, isValid, usageCount);
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

            return MapToDto(voucher, isValid, usageCount);
        }

        public async Task<VoucherResponseDto> CreateVoucherAsync(CreateVoucherRequestDto request)
        {
            // Normalize DiscountType: Amount -> Fixed
            var discountType = NormalizeDiscountType(request.DiscountType);

            // Code unique
            var existing = await _voucherRepository.GetByCodeAsync(request.Code.Trim());
            if (existing != null)
                throw new InvalidOperationException($"Voucher with code '{request.Code}' already exists");

            // EndTime > StartTime
            if (request.EndTime <= request.StartTime)
                throw new ArgumentException("End date must be after start date");

            // Value > 0 (already validated by DTO Range)
            // MaxDiscountValue: when Percent, optional; when Fixed/Amount, ignore

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                DiscountType = discountType,
                Value = request.Value,
                MinOrderValue = request.MinOrderValue,
                MaxDiscountValue = discountType == "Percent" ? request.MaxDiscountValue : null,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                TotalUsageLimit = 0,
                PerUserLimit = 1,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _voucherRepository.AddAsync(voucher);
            return MapToDto(created, false, 0);
        }

        public async Task<VoucherResponseDto?> UpdateVoucherAsync(Guid id, UpdateVoucherRequestDto request)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null)
                return null;

            if (request.Code != null)
            {
                var trimmedCode = request.Code.Trim();
                if (trimmedCode != voucher.Code)
                {
                    var existing = await _voucherRepository.GetByCodeAsync(trimmedCode);
                    if (existing != null)
                        throw new InvalidOperationException($"Voucher with code '{trimmedCode}' already exists");
                    voucher.Code = trimmedCode;
                }
            }

            if (request.Name != null)
                voucher.Name = request.Name.Trim();

            if (request.DiscountType != null)
            {
                voucher.DiscountType = NormalizeDiscountType(request.DiscountType);
                voucher.MaxDiscountValue = voucher.DiscountType == "Percent" ? request.MaxDiscountValue : null;
            }
            else if (request.MaxDiscountValue.HasValue && voucher.DiscountType == "Percent")
            {
                voucher.MaxDiscountValue = request.MaxDiscountValue;
            }

            if (request.Value.HasValue)
                voucher.Value = request.Value.Value;

            if (request.MinOrderValue.HasValue)
                voucher.MinOrderValue = request.MinOrderValue.Value;

            if (request.StartTime.HasValue)
                voucher.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                voucher.EndTime = request.EndTime.Value;

            if (voucher.EndTime <= voucher.StartTime)
                throw new ArgumentException("End date must be after start date");

            if (request.IsActive.HasValue)
                voucher.IsActive = request.IsActive.Value;

            voucher.UpdatedAt = DateTime.UtcNow;
            var updated = await _voucherRepository.UpdateAsync(voucher);

            var usageCount = await _voucherRepository.GetUsageCountAsync(updated.Id);
            var now = DateTime.UtcNow;
            var isValid = updated.IsActive && updated.StartTime <= now && updated.EndTime >= now
                        && (updated.TotalUsageLimit == 0 || usageCount < updated.TotalUsageLimit);

            return MapToDto(updated, isValid, usageCount);
        }

        public async Task<bool> DeleteVoucherAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null)
                return false;

            var usageCount = await _voucherRepository.GetUsageCountAsync(id);
            if (usageCount > 0)
                throw new InvalidOperationException("Cannot delete voucher that has been used");

            return await _voucherRepository.DeleteAsync(id);
        }

        public async Task<VoucherResponseDto?> ToggleActiveAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null)
                return null;

            voucher.IsActive = !voucher.IsActive;
            voucher.UpdatedAt = DateTime.UtcNow;
            var updated = await _voucherRepository.UpdateAsync(voucher);

            var usageCount = await _voucherRepository.GetUsageCountAsync(updated.Id);
            var now = DateTime.UtcNow;
            var isValid = updated.IsActive && updated.StartTime <= now && updated.EndTime >= now
                        && (updated.TotalUsageLimit == 0 || usageCount < updated.TotalUsageLimit);

            return MapToDto(updated, isValid, usageCount);
        }

        private static string NormalizeDiscountType(string discountType)
        {
            return discountType.Trim().Equals("Amount", StringComparison.OrdinalIgnoreCase) ? "Fixed" : discountType.Trim();
        }

        private static VoucherResponseDto MapToDto(Voucher voucher, bool isValid, int currentUsage)
        {
            return new VoucherResponseDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                DiscountType = voucher.DiscountType,
                Value = voucher.Value,
                StartTime = voucher.StartTime,
                EndTime = voucher.EndTime,
                MinOrderValue = voucher.MinOrderValue,
                MaxDiscountValue = voucher.MaxDiscountValue,
                TotalUsageLimit = voucher.TotalUsageLimit,
                PerUserLimit = voucher.PerUserLimit,
                IsActive = voucher.IsActive,
                IsValid = isValid,
                CurrentUsage = currentUsage,
                CreatedAt = voucher.CreatedAt,
                UpdatedAt = voucher.UpdatedAt
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

            // Calculate discount (support Percent, Fixed, Amount)
            var discountType = voucher.DiscountType.Equals("Amount", StringComparison.OrdinalIgnoreCase) ? "Fixed" : voucher.DiscountType;

            decimal discountAmount = 0;
            if (discountType == "Percent")
            {
                discountAmount = subtotalEligible * (voucher.Value / 100);
                if (voucher.MaxDiscountValue.HasValue && voucher.MaxDiscountValue.Value > 0)
                {
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountValue.Value);
                }
            }
            else if (discountType == "Fixed")
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
