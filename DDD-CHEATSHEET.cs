/**
 *  ╔══════════════════════════════════════════════════════════════════════════╗
 *  ║                    CHEAT SHEET: DDD PHỐI HỢP                            ║
 *  ║              (Cách phối hợp Domain + Application Layer)                 ║
 *  ╚══════════════════════════════════════════════════════════════════════════╝
 */

// ─────────────────────────────────────────────────────────────────────────────
// 1️⃣  DOMAIN LAYER - Chứa Business Logic Cốt Lõi
// ─────────────────────────────────────────────────────────────────────────────

namespace repodemo.Domain.Specifications
{
    // ✓ Static class chứa các quy tắc kinh doanh
    // ✓ Không phụ thuộc Repository hay Infrastructure
    // ✓ Dễ test, tái sử dụng
    public static class OrderBusinessRules
    {
        // Quy tắc 1: Số lượng phải > 0
        public static (bool IsValid, string Error) ValidateQuantity(int qty)
        {
            return qty > 0 
                ? (true, "") 
                : (false, "Số lượng phải > 0");
        }

        // Quy tắc 2: Số lượng <= Stock
        public static (bool IsValid, string Error) ValidateStock(
            int qty, int stock, string productName)
        {
            return qty <= stock
                ? (true, "")
                : (false, $"{productName} vượt quá kho");
        }

        // Quy tắc 3: Giá >= 0
        public static (bool IsValid, string Error) ValidatePrice(decimal price)
        {
            return price >= 0
                ? (true, "")
                : (false, "Giá không thể âm");
        }
    }
}

namespace repodemo.Domain.Services
{
    // ✓ Domain Service - Xử lý logic phức tạp
    // ✓ NỘI DỰA VÀO: OrderBusinessRules
    // ✓ KHÔNG phụ thuộc: Repository, DB, HttpContext
    public class OrderDomainService
    {
        // Phương thức này validate tất cả rules liên quan item
        // Có thể dùng từ:
        //   - OrderService.AddOrder()
        //   - OrderService.UpdateOrder()
        //   - CartService.AddToCart() (nếu cần)
        public (bool IsValid, string Error) ValidateOrderItem(
            string name,
            int qty,
            int stock,
            decimal price)
        {
            // Check Rule 1: Quantity > 0
            var (qtyOk, qtyErr) = OrderBusinessRules.ValidateQuantity(qty);
            if (!qtyOk) return (false, qtyErr);

            // Check Rule 2: Quantity <= Stock
            var (stockOk, stockErr) = OrderBusinessRules.ValidateStock(qty, stock, name);
            if (!stockOk) return (false, stockErr);

            // Check Rule 3: Price >= 0
            var (priceOk, priceErr) = OrderBusinessRules.ValidatePrice(price);
            if (!priceOk) return (false, priceErr);

            return (true, "");
        }

        // Tính tổng tiền = Σ(Quantity × Price)
        // ✓ Business logic của domain
        public decimal CalculateTotal(List<(int Qty, decimal Price)> items)
        {
            return items.Sum(x => x.Qty * x.Price);
        }
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// 2️⃣  APPLICATION LAYER - Điều Phối Use Case
// ─────────────────────────────────────────────────────────────────────────────

public class OrderService : IOrderService
{
    // ✓ Inject Domain Service
    // ✓ Inject Repositories
    // ✓ Inject UnitOfWork (transaction manager)
    private readonly OrderDomainService _orderDomainService;
    private readonly OrderRepository _orderRepository;
    private readonly CartRepository _cartRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly JwtService _jwtService;

    public OrderService(
        OrderRepository orderRepository,
        CartRepository cartRepository,
        UnitOfWork unitOfWork,
        JwtService jwtService,
        OrderDomainService orderDomainService)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _orderDomainService = orderDomainService;  // ← DOMAIN SERVICE
    }

    public async Task<ResponseData<OrderDTO>> AddOrder(string token, CartDTO cartDTO)
    {
        try
        {
            // ════════════════════════════════════════════════════════════════
            // BƯỚC 1: XÁC THỰC HTTP CONTEXT (Application Concern)
            // ════════════════════════════════════════════════════════════════
            string userID = _jwtService.DecodePayloadToken(token);
            if (string.IsNullOrEmpty(userID))
                return Error(401, "Unauthorized");

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 2: LẤY DỮ LIỆU TỪ REPOSITORY (Infrastructure Access)
            // ════════════════════════════════════════════════════════════════
            var cart = await _cartRepository.GetAsync(cartDTO.CartId);
            if (cart == null || cart.UserId.ToString() != userID)
                return Error(400, "Cart không hợp lệ");

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 3: VALIDATE DỮ LIỆU INPUT (Basic Validation)
            // ════════════════════════════════════════════════════════════════
            if (!cart.Items.Any())
                return Error(400, "Giỏ hàng trống");

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 4: VALIDATE BUSINESS LOGIC (Domain Service)
            // ════════════════════════════════════════════════════════════════
            // ✓ GỌI DOMAIN SERVICE để validate từng item
            foreach (var item in cart.Items)
            {
                var (isValid, error) = _orderDomainService.ValidateOrderItem(
                    item.ProductName,
                    item.Quantity,
                    item.AvailableStock,
                    item.Price);

                if (!isValid)
                    return Error(400, error);
            }

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 5: TÍNH TOÁN (Business Logic)
            // ════════════════════════════════════════════════════════════════
            // ✓ GỌI DOMAIN SERVICE để tính tổng tiền
            decimal totalAmount = _orderDomainService.CalculateTotal(
                cart.Items.Select(i => (i.Quantity, i.Price)).ToList());

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 6: TẠO ENTITY (Model Creation)
            // ════════════════════════════════════════════════════════════════
            var order = new Order
            {
                BuyerId = Guid.Parse(userID),
                CreatedAt = DateTime.Now,
                TotalAmount = totalAmount,  // ← Được tính từ Domain Service
                Alias = GenerateAlias(userID, cart.Items),
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    VariantId = i.VariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList()
            };

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 7: LƯU VÀO DATABASE (Persistence)
            // ════════════════════════════════════════════════════════════════
            await _orderRepository.AddAsync(order);
            await _cartRepository.RemoveAsync(cart);
            await _unitOfWork.SaveChangesAsync();  // ← Transaction

            // ════════════════════════════════════════════════════════════════
            // BƯỚC 8: RETURN RESPONSE (HTTP Concern)
            // ════════════════════════════════════════════════════════════════
            return Success(new OrderDTO
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(MapToDTO).ToList()
            });
        }
        catch (Exception ex)
        {
            return Error(500, ex.Message);
        }
    }

    private ResponseData<OrderDTO> Error(int code, string msg)
        => new() { statusCode = code, message = msg, data = null };

    private ResponseData<OrderDTO> Success(OrderDTO dto)
        => new() { statusCode = 200, message = "Success", data = dto };

    private string GenerateAlias(string userId, List<CartItem> items)
        => $"order-{userId}-{string.Join("-", items.Select(i => i.ProductName))}";

    private OrderItemDTO MapToDTO(OrderItem oi)
        => new() { VariantId = oi.VariantId, Quantity = oi.Quantity, Price = oi.UnitPrice };
}


// ─────────────────────────────────────────────────────────────────────────────
// 3️⃣  DEPENDENCY INJECTION - Đăng ký trong Program.cs
// ─────────────────────────────────────────────────────────────────────────────

/*
// Program.cs

// Domain Services - Không phụ thuộc Infrastructure
builder.Services.AddScoped<OrderDomainService>();

// Application Services - Phụ thuộc Domain Services + Repositories
builder.Services.AddScoped<IOrderService, OrderService>();

// Repositories - Infrastructure
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<CartRepository>();

// Transaction Manager
builder.Services.AddScoped<UnitOfWork>();
*/


// ─────────────────────────────────────────────────────────────────────────────
// 4️⃣  FLOW DIAGRAM - Luồng xử lý
// ─────────────────────────────────────────────────────────────────────────────

/*

    API REQUEST
        ↓ token, cartDTO
    
    ┌─────────────────────────────────────────────────────────────┐
    │ OrderService.AddOrder()  (Application Layer)                │
    │                                                             │
    │  1. Xác thực token          [JwtService]                   │
    │     ↓                                                        │
    │  2. Lấy giỏ hàng            [CartRepository]                │
    │     ↓                                                        │
    │  3. Kiểm tra giỏ trống      [Basic validation]              │
    │     ↓                                                        │
    │  4. ┌──────────────────────────────────────────────────┐   │
    │     │ FOR EACH item:                                   │   │
    │     │   ↓                                              │   │
    │     │   OrderDomainService.ValidateOrderItem()        │   │
    │     │   └─ (Domain Layer - Validate business rules)   │   │
    │     │      ├─ ValidateQuantity()                      │   │
    │     │      ├─ ValidateStock()                         │   │
    │     │      └─ ValidatePrice()                         │   │
    │     └──────────────────────────────────────────────────┘   │
    │     ↓                                                        │
    │  5. OrderDomainService.CalculateTotal()                    │
    │     └─ (Domain Layer - Tính toán business logic)           │
    │     ↓                                                        │
    │  6. new Order() - Tạo entity                               │
    │     ↓                                                        │
    │  7. _orderRepository.AddAsync()                            │
    │     _cartRepository.RemoveAsync()                          │
    │     _unitOfWork.SaveChangesAsync()                         │
    │     ↓ (Persistence)                                        │
    │  8. Return Response                                         │
    │                                                             │
    └─────────────────────────────────────────────────────────────┘
        ↓ ResponseData<OrderDTO>
    
    API RESPONSE


*/


// ─────────────────────────────────────────────────────────────────────────────
// 5️⃣  CÓ THỂ DÙNG DOMAIN SERVICE Ở ĐÂU?
// ─────────────────────────────────────────────────────────────────────────────

/*

OrderDomainService có thể dùng từ:

1. ✓ OrderService.AddOrder()
   └─ Tạo đơn hàng mới từ giỏ hàng

2. ✓ OrderService.UpdateOrder()
   └─ Cập nhật items trong đơn hàng

3. ✓ OrderService.AddItemToOrder()
   └─ Thêm item vào đơn hàng (nếu order chưa confirmed)

4. ✓ CartService.AddToCart()
   └─ Validate trước khi thêm vào giỏ (tái sử dụng ValidateOrderItem)

5. ✓ BulkOrderService.CreateBulkOrders()
   └─ Tạo nhiều đơn hàng cùng một lúc

6. ✓ WebhookService.ProcessOrderWebhook()
   └─ Xử lý API từ bên thứ 3 để tạo order

7. ✓ BackgroundJob.RecalculateOrderPrices()
   └─ Tính lại giá order trong background

▶ Tất cả đều dùng CHUNG một Domain Service
▶ Không lặp lại code validation/calculation
▶ Consistency across the system

*/


// ─────────────────────────────────────────────────────────────────────────────
// 6️⃣  UNIT TEST - Dễ dàng test Domain Service
// ─────────────────────────────────────────────────────────────────────────────

/*

[TestClass]
public class OrderDomainServiceTests
{
    private OrderDomainService _service;
    
    [TestInitialize]
    public void Setup()
    {
        // Create domain service without Repository
        _service = new OrderDomainService();
    }
    
    [TestMethod]
    public void ValidateQuantity_LessThanOrEqualZero_ReturnsFalse()
    {
        var (isValid, error) = _service.ValidateOrderItem(
            "Product1", 0, 10, 100m);
        Assert.IsFalse(isValid);
        Assert.AreEqual("Số lượng phải > 0", error);
    }
    
    [TestMethod]
    public void ValidateStock_ExceedsAvailable_ReturnsFalse()
    {
        var (isValid, error) = _service.ValidateOrderItem(
            "Product1", 15, 10, 100m);  // qty > stock
        Assert.IsFalse(isValid);
    }
    
    [TestMethod]
    public void CalculateTotal_CorrectFormula()
    {
        var items = new[] { (Qty: 5, Price: 100m), (Qty: 3, Price: 50m) };
        var total = _service.CalculateTotal(items.ToList());
        // 5*100 + 3*50 = 500 + 150 = 650
        Assert.AreEqual(650m, total);
    }
}

▶ Không cần Mock Repository
▶ Test chỉ domain logic
▶ Nhanh, rõ ràng, dễ viết

*/
