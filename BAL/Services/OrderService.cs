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

                var totalAmount = subtotal - discountAmount;

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
                    Notes = request.Notes,
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
