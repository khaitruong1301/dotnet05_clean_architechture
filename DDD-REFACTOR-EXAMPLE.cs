/// <summary>
/// EXAMPLE: OrderService refactored với DDD pattern
/// So sánh: BEFORE và AFTER
/// </summary>

/*
╔════════════════════════════════════════════════════════════════════════════╗
║                      BEFORE (No DDD Pattern)                               ║
╚════════════════════════════════════════════════════════════════════════════╝

❌ Tất cả business logic nằm trong OrderService
❌ Validation code lặp lại ở nhiều nơi
❌ Khó kiểm thử (tightly coupled với Repository)
❌ Khó bảo trì (logic phức tạp, không rõ ràng)

public class OrderService : IOrderService
{
    // ... DI ...
    
    public async Task<ResponseData<OrderDTO>> AddOrder(string token, CartDTO cartDTO)
    {
        try
        {
            // AUTHENTICATION
            string userID = _jwtService.DecodePayloadToken(token);
            if (string.IsNullOrEmpty(userID))
            {
                return new ResponseData<OrderDTO> { statusCode = 401, ... };
            }
            
            // LOAD DATA
            Cart? cart = await _cartRepository.SingleOrDefaultAsync(c => ...);
            if(cart == null)
            {
                return new ResponseData<OrderDTO> { statusCode = 400, ... };
            }
            
            // CREATE ORDER
            var order = new Order
            {
                BuyerId = Guid.Parse(userID),
                CreatedAt = DateTime.Now,
                TotalAmount = cartDTO.Items.Sum(item => item.Quantity),  // ❌ BUG: Quantity != Price
                Alias = FunctionUtility.GenerateSlug(...),
                ...
            };
            
            // VALIDATE ITEMS (INLINE - Not reusable)
            foreach (var item in cartDTO.Items)
            {
                var cartItem = await _cartItemRepository.SingleOrDefaultAsync(...);
                if(cartItem == null) return error;
                
                if(item.Quantity <= 0) return error; // ❌ Hardcoded validation
                
                if(item.Quantity > cartItem.Variant.Stock) return error;  // ❌ Logic ở đây
                
                // ❌ Duplicate code...
            }
            
            // ... save to DB ...
        }
        catch (Exception ex)
        {
            return error;
        }
    }
}


╔════════════════════════════════════════════════════════════════════════════╗
║                    AFTER (Proper DDD Pattern)                              ║
╚════════════════════════════════════════════════════════════════════════════╝

✓ Domain Service chứa business logic
✓ Application Service điều phối use case
✓ Code reusable, testable, maintainable

*/

/// <summary>
/// DOMAIN LAYER - OrderBusinessRules
/// - Chứa các quy tắc kinh doanh
/// - Static methods, không phụ thuộc Repository
/// - Dễ unit test
/// </summary>
public static class OrderBusinessRules
{
    public static (bool IsValid, string ErrorMessage) ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            return (false, "Số lượng sản phẩm phải lớn hơn 0");
        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateStock(int quantity, int availableStock, string productName)
    {
        if (quantity > availableStock)
            return (false, $"Số lượng sản phẩm {productName} vượt quá kho. Còn: {availableStock}");
        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidatePrice(decimal price)
    {
        if (price < 0)
            return (false, "Giá tidak thể âm");
        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateItemCount(int count)
    {
        if (count == 0)
            return (false, "Giỏ hàng trống");
        return (true, string.Empty);
    }
}

/// <summary>
/// DOMAIN LAYER - OrderDomainService
/// - Xử lý business logic liên quan Order
/// - Không phụ thuộc vào Repository
/// - Có thể dùng từ nhiều Application Services
/// </summary>
public class OrderDomainService
{
    /// <summary>
    /// Validate một item với business rules
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateOrderItem(
        string productName, 
        int quantity, 
        int availableStock, 
        decimal unitPrice)
    {
        // Rule 1: Quantity > 0
        var (qtyValid, qtyError) = OrderBusinessRules.ValidateQuantity(quantity);
        if (!qtyValid) 
            return (false, qtyError);

        // Rule 2: Quantity <= Stock
        var (stockValid, stockError) = OrderBusinessRules.ValidateStock(quantity, availableStock, productName);
        if (!stockValid) 
            return (false, stockError);

        // Rule 3: Price >= 0
        var (priceValid, priceError) = OrderBusinessRules.ValidatePrice(unitPrice);
        if (!priceValid) 
            return (false, priceError);

        return (true, string.Empty);
    }

    /// <summary>
    /// Tính tổng tiền chính xác (Quantity * Price, NOT just Quantity)
    /// </summary>
    public decimal CalculateOrderTotal(List<(int Quantity, decimal UnitPrice)> items)
    {
        return items.Sum(item => item.Quantity * item.UnitPrice);  // ✓ Đúng công thức
    }
}

/// <summary>
/// APPLICATION LAYER - OrderService
/// - Điều phối use case (CreateOrder)
/// - Gọi Domain Service để validate business logic
/// - Gọi Repository để lấy/lưu dữ liệu
/// - Quản lý transaction
/// </summary>
public class OrderService : IOrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CartRepository _cartRepository;
    private readonly CartItemRepository _cartItemRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly JwtService _jwtService;
    
    // ✓ INJECT Domain Service
    private readonly OrderDomainService _orderDomainService;

    public OrderService(
        OrderRepository orderRepository,
        CartRepository cartRepository,
        CartItemRepository cartItemRepository,
        UnitOfWork unitOfWork,
        JwtService jwtService,
        OrderDomainService orderDomainService)  // ← Domain Service
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _orderDomainService = orderDomainService;  // ← Gán DI
    }

    public async Task<ResponseData<OrderDTO>> AddOrder(string token, CartDTO cartDTO)
    {
        try
        {
            // ════════════════════════════════════════════════════════════════
            // STEP 1: AUTHENTICATION - Xác thực token
            // ════════════════════════════════════════════════════════════════
            string userID = _jwtService.DecodePayloadToken(token);
            if (string.IsNullOrEmpty(userID))
                return BuildErrorResponse(401, "Unauthorized");

            // ════════════════════════════════════════════════════════════════
            // STEP 2: LOAD DATA - Lấy giỏ hàng từ DB
            // ════════════════════════════════════════════════════════════════
            Cart? cart = await _cartRepository.SingleOrDefaultAsync(
                c => c.UserId.ToString() == userID && c.Id == cartDTO.CartId);
            if (cart == null)
                return BuildErrorResponse(400, "Giỏ hàng không hợp lệ");

            // ════════════════════════════════════════════════════════════════
            // STEP 3: BASIC VALIDATION - Kiểm tra input
            // ════════════════════════════════════════════════════════════════
            var (isValidCount, countError) = OrderBusinessRules.ValidateItemCount(cartDTO.Items.Count);
            if (!isValidCount)
                return BuildErrorResponse(400, countError);

            // ════════════════════════════════════════════════════════════════
            // STEP 4: DOMAIN VALIDATION - Gọi Domain Service validate each item
            // ════════════════════════════════════════════════════════════════
            foreach (var item in cartDTO.Items)
            {
                var cartItem = await _cartItemRepository.SingleOrDefaultAsync(
                    ci => ci.CartId == cartDTO.CartId && ci.VariantId == item.ProductVariantId);
                if (cartItem == null)
                    return BuildErrorResponse(400, $"Sản phẩm {item.Name} không tồn tại");

                // ✓ Gọi DOMAIN SERVICE để validate business rules
                var (isValid, errorMsg) = _orderDomainService.ValidateOrderItem(
                    item.Name,
                    item.Quantity,
                    cartItem.Variant.Stock,
                    item.Price);

                if (!isValid)
                    return BuildErrorResponse(400, errorMsg);
            }

            // ════════════════════════════════════════════════════════════════
            // STEP 5: CREATE ORDER - Tạo Order entity
            // ════════════════════════════════════════════════════════════════
            string orderAlias = FunctionUtility.GenerateSlug(
                $"order-{userID}-{string.Join("-", cartDTO.Items.Select(i => i.Name))}-{DateTime.Now.Ticks}");

            // ✓ Tính total price đúng: Quantity * Price (không phải chỉ Quantity)
            decimal totalAmount = _orderDomainService.CalculateOrderTotal(
                cartDTO.Items.Select(i => (i.Quantity, i.Price)).ToList());

            var order = new Order
            {
                BuyerId = Guid.Parse(userID),
                CreatedAt = DateTime.Now,
                TotalAmount = totalAmount,  // ✓ Đúng công thức
                Alias = orderAlias,
                AdditionalData = null,
                Deleted = false
            };

            // Map items
            order.OrderItems = cartDTO.Items.Select(item => new OrderItem
            {
                VariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                Alias = FunctionUtility.GenerateSlug($"orderitem-{item.Name}-{DateTime.Now.Ticks}"),
                AdditionalData = null,
                Deleted = false
            }).ToList();

            // ════════════════════════════════════════════════════════════════
            // STEP 6: PERSISTENCE - Lưu dữ liệu vào DB
            // ════════════════════════════════════════════════════════════════
            foreach (CartItem item in cart.CartItems)
                await _cartItemRepository.RemoveAsync(item);

            await _cartRepository.RemoveAsync(cart);
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // ════════════════════════════════════════════════════════════════
            // STEP 7: RESPONSE - Ánh xạ Entity → DTO và return
            // ════════════════════════════════════════════════════════════════
            return new ResponseData<OrderDTO>
            {
                statusCode = 200,
                data = MapToOrderDTO(order),
                message = "Thêm đơn hàng thành công",
                dateTime = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return BuildErrorResponse(500, $"Lỗi: {ex.Message}");
        }
    }

    private ResponseData<OrderDTO> BuildErrorResponse(int statusCode, string message)
    {
        return new ResponseData<OrderDTO>
        {
            statusCode = statusCode,
            data = null,
            message = message,
            dateTime = DateTime.Now
        };
    }

    private OrderDTO MapToOrderDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            CreateAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Alias = order.Alias,
            lstItem = order.OrderItems.Select(oi => new OrderItemDTO
            {
                OrderId = oi.OrderId,
                VariantId = oi.VariantId,
                VariantName = oi.Variant.VariantName,
                UrlImageVariant = oi.Variant.Image,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Alias = oi.Alias
            }).ToList()
        };
    }
}


/*
╔════════════════════════════════════════════════════════════════════════════╗
║                         BENEFITS OF THIS APPROACH                          ║
╚════════════════════════════════════════════════════════════════════════════╝

1. REUSABILITY
   - OrderDomainService.ValidateOrderItem() có thể dùng ở:
     • OrderService.AddOrder()
     • OrderService.UpdateOrder()
     • BulkOrderService.CreateBulkOrders()
     • Webhooks, APIs khác

2. TESTABILITY
   - Unit test OrderDomainService mà không cần Mock Repository
   
   [Test]
   public void ValidateQuantity_GreaterThanZero_ReturnsValid()
   {
       var result = _orderDomainService.ValidateOrderItem(
           "Product1", 5, 10, 100m);
       Assert.IsTrue(result.IsValid);
   }

3. MAINTAINABILITY
   - Business rules tập trung ở một nơi
   - Dễ sửa/thêm rules mới
   - Code rõ ràng, dễ hiểu

4. SEPARATION OF CONCERNS
   - Domain: WHAT (business logic)
   - Application: HOW (orchestration)
   - Infrastructure: WHERE (persistence)

5. CONSISTENCY
   - Validation logic luôn consistent
   - Không lặp lại code

*/
