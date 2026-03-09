using BAL.DTOs.Common;
using BAL.DTOs.Order;
using BAL.DTOs.Address;
using BAL.DTOs.Voucher;
using BAL.DTOs.Product;
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Order service implementation
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly TechStoreContext _context;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IAddressRepository addressRepository,
            ICartItemRepository cartItemRepository,
            TechStoreContext context)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _addressRepository = addressRepository;
            _cartItemRepository = cartItemRepository;
            _context = context;
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdWithFullDetailsAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<PagedResponse<OrderResponseDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var (orders, totalCount) = await _orderRepository.SearchByUserIdAsync(userId, pageNumber, pageSize);
            
            return new PagedResponse<OrderResponseDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResponse<OrderResponseDto>> SearchOrdersByOrderIdAsync(string orderIdSearch, int pageNumber, int pageSize)
        {
            var (orders, totalCount) = await _orderRepository.SearchByOrderIdAsync(orderIdSearch, pageNumber, pageSize);
            
            return new PagedResponse<OrderResponseDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResponse<OrderResponseDto>> SearchOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await GetOrdersByUserIdAsync(userId, pageNumber, pageSize);
        }

        public async Task<OrderResponseDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string paymentMethod, List<Guid>? cartItemIds = null, Guid? voucherId = null, string? notes = null, decimal shippingFee = 0, int? shippingServiceId = null)
        {
            // Get cart items - if cartItemIds is provided and not empty, only get those items
            IEnumerable<CartItem> cartItems;
            if (cartItemIds != null && cartItemIds.Count > 0)
            {
                // Get only selected cart items
                var allCartItems = await _cartItemRepository.GetByUserIdAsync(userId);
                cartItems = allCartItems.Where(item => cartItemIds.Contains(item.Id)).ToList();
                
                // Validate: ensure all requested cart items exist
                if (cartItems.Count() != cartItemIds.Count)
                {
                    var foundIds = cartItems.Select(item => item.Id).ToList();
                    var missingIds = cartItemIds.Except(foundIds).ToList();
                    throw new InvalidOperationException($"Some cart items not found: {string.Join(", ", missingIds)}");
                }
            }
            else
            {
                // Get all cart items if no specific items provided
                cartItems = await _cartItemRepository.GetByUserIdAsync(userId);
            }

            if (cartItems == null || !cartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty");
            }

            // Validate cart
            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product {item.ProductId} not found");
                }
                if (!product.IsActive)
                {
                    throw new InvalidOperationException($"Product {product.Name} is not active");
                }
                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Stock}");
                }
            }

            // Convert cart items to order items
            var orderItems = cartItems.Select(item => new CreateOrderItemRequestDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList();

            var createOrderRequest = new CreateOrderRequestDto
            {
                AddressId = addressId,
                VoucherId = voucherId,
                Notes = notes,
                PaymentMethod = paymentMethod,
                OrderItems = orderItems,
                ShippingFee = shippingFee,
                ShippingServiceId = shippingServiceId
            };

            var order = await CreateOrderAsync(userId, createOrderRequest);

            // Remove only the cart items that were checked out (not all items)
            // Important: Only remove the specific items that were included in the order
            var cartItemIdsToRemove = cartItems.Select(item => item.Id).ToList();
            
            // Double-check: ensure we only delete the items that were actually checked out
            if (cartItemIdsToRemove.Any())
            {
                await _cartItemRepository.DeleteCartItemsByIdsAsync(userId, cartItemIdsToRemove);
            }

            return order;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Business rule: Validate address belongs to user
                var address = await _addressRepository.GetByIdAndUserIdAsync(request.AddressId, userId);
                if (address == null)
                {
                    throw new InvalidOperationException("Address not found or does not belong to user");
                }

                // Business rule: Validate products and calculate totals
                decimal subtotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemRequest in request.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Product {itemRequest.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new InvalidOperationException($"Product {product.Name} is not active");
                    }

                    if (product.Stock < itemRequest.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Stock}");
                    }

                    var unitPrice = product.DiscountPrice ?? product.Price;
                    subtotal += unitPrice * itemRequest.Quantity;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = itemRequest.ProductId,
                        Quantity = itemRequest.Quantity,
                        UnitPrice = unitPrice,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);

                    // Update product stock
                    product.Stock -= itemRequest.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product);
                }

                // Business rule: Apply voucher if provided
                decimal discountAmount = 0;
                Guid? voucherId = null;
                if (request.VoucherId.HasValue)
                {
                    // Voucher application logic would go here (similar to VoucherService)
                    // For now, we'll just store the voucher ID
                    voucherId = request.VoucherId.Value;
                }

                var shippingFee = request.ShippingFee;
                var totalAmount = subtotal - discountAmount + shippingFee;

                var paymentMethod = request.PaymentMethod ?? "COD";
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AddressId = request.AddressId,
                    Status = "Pending",
                    Subtotal = subtotal,
                    DiscountAmount = discountAmount,
                    VoucherId = voucherId,
                    TotalAmount = totalAmount,
                    ShippingFee = shippingFee,
                    ShippingServiceId = request.ShippingServiceId,
                    Notes = request.Notes,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = paymentMethod == "Online" ? "Pending" : "COD",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderRepository.AddAsync(order);

                // Add order items
                foreach (var item in orderItems)
                {
                    item.OrderId = createdOrder.Id;
                    await _orderItemRepository.AddAsync(item);
                }

                await transaction.CommitAsync();

                var orderWithDetails = await _orderRepository.GetByIdWithFullDetailsAsync(createdOrder.Id);
                return MapToDto(orderWithDetails!);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResponseDto?> UpdateOrderAsync(Guid id, UpdateOrderRequestDto request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return null;

            if (request.Status != null)
            {
                // Business rule: Validate status transition
                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "SUCCESS", "Cancelled" };
                if (!validStatuses.Contains(request.Status))
                {
                    throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
                }

                order.Status = request.Status;
                
                if (request.Status == "SUCCESS" && order.DeliveredAt == null)
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }
            }

            if (request.Notes != null)
                order.Notes = request.Notes;

            order.UpdatedAt = DateTime.UtcNow;
            var updated = await _orderRepository.UpdateAsync(order);
            var orderWithDetails = await _orderRepository.GetByIdWithFullDetailsAsync(updated.Id);
            return MapToDto(orderWithDetails!);
        }

        public async Task<bool> CancelOrderByUserAsync(Guid orderId, Guid userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            // Business rule: User can only cancel orders with status "Pending" or "Processing"
            if (order.Status != "Pending" && order.Status != "Processing")
            {
                throw new InvalidOperationException($"Cannot cancel order. Current status: {order.Status}. Only Pending or Processing orders can be cancelled by user.");
            }

            order.Status = "Cancelled";
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledBy = userId;
            order.UpdatedAt = DateTime.UtcNow;

            // Business rule: Restore product stock
            var orderItems = await _orderItemRepository.FindAsync(oi => oi.OrderId == orderId);
            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product);
                }
            }

            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderByStaffAsync(Guid orderId, Guid staffId, string cancelReason)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Business rule: Staff/Admin can cancel any order
            order.Status = "Cancelled";
            order.CancelReason = cancelReason;
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledBy = staffId;
            order.UpdatedAt = DateTime.UtcNow;

            // Business rule: Restore product stock if order was not yet delivered
            if (order.Status != "SUCCESS" && order.DeliveredAt == null)
            {
                var orderItems = await _orderItemRepository.FindAsync(oi => oi.OrderId == orderId);
                foreach (var item in orderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            await _orderRepository.UpdateAsync(order);
            return true;
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                Address = order.Address != null ? new AddressResponseDto
                {
                    Id = order.Address.Id,
                    UserId = order.Address.UserId,
                    RecipientName = order.Address.RecipientName,
                    PhoneNumber = order.Address.PhoneNumber,
                    AddressLine1 = order.Address.AddressLine1,
                    AddressLine2 = order.Address.AddressLine2,
                    Ward = order.Address.Ward,
                    District = order.Address.District,
                    City = order.Address.City,
                    IsPrimary = order.Address.IsPrimary,
                    ProvinceId = order.Address.ProvinceId,
                    DistrictId = order.Address.DistrictId,
                    WardCode = order.Address.WardCode,
                    Latitude = order.Address.Latitude,
                    Longitude = order.Address.Longitude,
                    AddressNote = order.Address.AddressNote,
                    CreatedAt = order.Address.CreatedAt,
                    UpdatedAt = order.Address.UpdatedAt
                } : null,
                Status = order.Status,
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                VoucherId = order.VoucherId,
                Voucher = order.Voucher != null ? new VoucherResponseDto
                {
                    Id = order.Voucher.Id,
                    Code = order.Voucher.Code,
                    DiscountType = order.Voucher.DiscountType,
                    Value = order.Voucher.Value,
                    StartTime = order.Voucher.StartTime,
                    EndTime = order.Voucher.EndTime,
                    MinOrderValue = order.Voucher.MinOrderValue,
                    TotalUsageLimit = order.Voucher.TotalUsageLimit,
                    PerUserLimit = order.Voucher.PerUserLimit,
                    IsActive = order.Voucher.IsActive
                } : null,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                CancelReason = order.CancelReason,
                CancelledAt = order.CancelledAt,
                CancelledBy = order.CancelledBy,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                DeliveredAt = order.DeliveredAt,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                VnPayTransactionNo = order.VnPayTransactionNo,
                PaymentDate = order.PaymentDate,
                ShippingFee = order.ShippingFee,
                GhnOrderCode = order.GhnOrderCode,
                ExpectedDeliveryTime = order.ExpectedDeliveryTime,
                ShippingServiceId = order.ShippingServiceId,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Product = oi.Product != null ? new ProductResponseDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Price = oi.Product.Price,
                        DiscountPrice = oi.Product.DiscountPrice,
                        Stock = oi.Product.Stock,
                        ImageUrl = oi.Product.ImageUrl,
                        CategoryId = oi.Product.CategoryId,
                        BrandId = oi.Product.BrandId,
                        IsActive = oi.Product.IsActive,
                        IsOnSale = oi.Product.IsOnSale,
                        NoVoucherTag = oi.Product.NoVoucherTag,
                        CreatedAt = oi.Product.CreatedAt,
                        UpdatedAt = oi.Product.UpdatedAt
                    } : null,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Status = oi.Status,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt
                }).ToList() ?? new List<OrderItemResponseDto>()
            };
        }
    }
}
